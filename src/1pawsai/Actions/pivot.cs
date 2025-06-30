using System;
using System.Collections.Generic;
using System.Linq;
using FMOD;
using FSPRO;
using Nickel;

namespace Starhunters.Pawsai.Actions;

public class APivot : AMove
{
    /// <summary>
    /// No.
    /// </summary>
    public new bool fromEvade => false;

    /// <summary>
    /// Doesn't matter whether it targets player or not, it'll do the same thing
    /// </summary>
    public new bool targetPlayer => true;

    /// <summary>
    /// To reduce confusion (on my end). Inward movement is positive, outward movement is negative. For readability, amount and outward bool is used instead.
    /// </summary>
    public new int dir { get; private set; }
    public int amount;
    public bool outward = false;


    public override void Begin(G g, State s, Combat c)
    {
        bool debugMode = FeatureFlags.Debug && Input.shift;

        (dir, int otherDir) = GetActualMovement(s, c, debugMode);
        if (dir == 0 && otherDir == 0)
        {
            timer = 0;
            return;
        }

        timer *= 0.5;

        if (c.otherShip.Get(Status.strafe) > 0)
        {
            c.QueueImmediate(new AAttack
            {
                damage = Card.GetActualDamage(s, c.otherShip.Get(Status.strafe), true, null),
                targetPlayer = true,
                fast = true
            });
        }

        if (s.ship.Get(Status.strafe) > 0)
        {
            c.QueueImmediate(new AAttack
            {
                damage = Card.GetActualDamage(s, s.ship.Get(Status.strafe), false, null),
                targetPlayer = false,
                fast = true,
                storyFromStrafe = true
            });
        }

        if (isTeleport)
        {
            PawsaiMovingHelper.DoTeleportationVisualMagic(g, s, c, s.ship, dir);
            PawsaiMovingHelper.DoTeleportationVisualMagic(g, s, c, c.otherShip, otherDir);
        }

        s.ship.x += dir;
        c.otherShip.x += otherDir;

        if (PawsaiMovingHelper.CheckAgainstWall(g, s, c, s.ship, out GUID? aguid) | PawsaiMovingHelper.CheckAgainstWall(g, s, c, c.otherShip, out GUID? bguid))
        {
            // Anti-duplicate sounds
            if (aguid is GUID a && bguid is GUID b && a == b)
            {
                Audio.Play(a, true);
            }
            else
            {
                if (aguid is not null) Audio.Play(aguid, true);
                if (bguid is not null) Audio.Play(bguid, true);
            }
        }

        if (isTeleport)
        {
            s.ship.xLerped = s.ship.x;
            c.otherShip.xLerped = c.otherShip.x;
        }

        Audio.Play(Event.Move, true);

        s.storyVars.shipsDontOverlapAtAll = s.ship.x >= c.otherShip.x + c.otherShip.parts.Count || s.ship.x + s.ship.parts.Count <= c.otherShip.x;
    }

    /// <summary>
    /// Gets the actual movement of the two ships, depending on their position or anchor point
    /// </summary>
    /// <param name="state"></param>
    /// <param name="combat"></param>
    /// <param name="debugMode"></param>
    /// <returns></returns>
    private (int playerMove, int enemyMove) GetActualMovement(State state, Combat combat, bool debugMode = false)
    {
        int playerMovement = amount;
        int enemyMovement = amount;

        // Choose a direction
        int playerMultiplier = 0;
        int enemyMultiplier = 0;
        double playerCenter = (state.ship.x + (state.ship.x + state.ship.parts.Count - 1)) / 2.0;
        double enemyCenter = (combat.otherShip.x + (combat.otherShip.x + combat.otherShip.parts.Count - 1)) / 2.0;
        // If player is left of enemy
        if (playerCenter < enemyCenter)
        {
            playerMultiplier = outward ? -1 : 1;  // Outward is always opposite to inward
            enemyMultiplier = outward ? 1 : -1;
        }
        // If player is right of enemy
        else if (playerCenter > enemyCenter)
        {
            playerMultiplier = outward ? 1 : -1;
            enemyMultiplier = outward ? -1 : 1;
        }
        else if (outward)  // If both centered but going outwards
        {
            if (!RandomMeansLeft(state) && state.rngActions.Next() < 0.5)
            {
                playerMultiplier = 1;
                enemyMultiplier = -1;
            }
            else
            {
                playerMultiplier = -1;
                enemyMultiplier = 1;
            }
        }
        // anchor finding code, move toward/away from anchor

        if (state.ship.Get(Status.hermes) > 0) playerMovement += state.ship.Get(Status.hermes);
        if (playerMovement > 0 && PawsaiMovingHelper.CheckIfShipCantMove(state, combat, state.ship, debugMode))
        {
            Audio.Play(Event.Status_PowerDown, true);
            state.ship.shake += 1.0;
            playerMovement = 0;
        }

        if (combat.otherShip.Get(Status.hermes) > 0) enemyMovement += combat.otherShip.Get(Status.hermes);
        if (enemyMovement > 0 && PawsaiMovingHelper.CheckIfShipCantMove(combat.otherShip, debugMode))
        {
            Audio.Play(Event.Status_PowerDown, true);
            combat.otherShip.shake += 1.0;
            enemyMovement = 0;
        }

        return (playerMovement * playerMultiplier, enemyMovement * enemyMultiplier);
    }

    /// <summary>
    /// Just gets the direction of the movement, for tooltip/icon purposes
    /// </summary>
    /// <param name="s"></param>
    /// <returns>positive = right, negative = left</returns>
    private int GetAdvertisedMovement(State s)
    {
        int a = amount;
        if (s.ship.Get(Status.hermes) > 0 && !ignoreHermes)
        {
            a += s.ship.Get(Status.hermes);
        }
        if (s.route is Combat c)
        {
            double playerCenter = (s.ship.x + (s.ship.x + s.ship.parts.Count - 1)) / 2.0;
            double enemyCenter = (c.otherShip.x + (c.otherShip.x + c.otherShip.parts.Count - 1)) / 2.0;

            if ((!outward && playerCenter > enemyCenter) || (outward && playerCenter < enemyCenter))
            {
                return -a;
            }
        }
        return a;
    }

    public override List<Tooltip> GetTooltips(State s)
    {
        return
        [
            new GlossaryTooltip("randomMirrorMovement")
            {
                Title = ModEntry.Instance.Localizations.Localize(["Pawsai", "action", outward? "PivotOut" : "PivotIn", "title"]),
                Icon = (GetAdvertisedMovement(s), outward) switch
                {
                    (<0, false) => ModEntry.Instance.Action_PivotIn_Left,
                    (0, false) => ModEntry.Instance.Action_PivotIn_Zero,
                    (<0, true) => ModEntry.Instance.Action_PivotOut_Left,
                    (>0, true) => ModEntry.Instance.Action_PivotOut_Right,
                    (_, true) => ModEntry.Instance.Action_PivotOut_Zero,
                    (_, _) => ModEntry.Instance.Action_PivotIn_Right
                },
                IsWideIcon = true,
                Description = ModEntry.Instance.Localizations.Localize(["Pawsai", "action", outward? "PivotOut" : "PivotIn", "desc"], new List<string>(){Math.Abs(GetAdvertisedMovement(s)).ToString()})
            }
        ];
    }

    public override Icon? GetIcon(State s)
    {
        return new Icon
        (
            (GetAdvertisedMovement(s), outward) switch
            {
                ( < 0, false) => ModEntry.Instance.Action_PivotIn_Left,
                (0, false) => ModEntry.Instance.Action_PivotIn_Zero,
                ( < 0, true) => ModEntry.Instance.Action_PivotOut_Left,
                ( > 0, true) => ModEntry.Instance.Action_PivotOut_Right,
                (_, true) => ModEntry.Instance.Action_PivotOut_Zero,
                (_, _) => ModEntry.Instance.Action_PivotIn_Right
            },
            Math.Abs(GetAdvertisedMovement(s)),
            Colors.textMain,
            false
        );
    }
}