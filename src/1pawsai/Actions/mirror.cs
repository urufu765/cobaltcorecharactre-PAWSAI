using System;
using System.Collections.Generic;
using System.Linq;
using FMOD;
using FSPRO;
using Microsoft.Extensions.Logging;
using Nickel;

namespace Starhunters.Pawsai.Actions;

public class AMirrorMove : AMove
{
    /// <summary>
    /// Doesn't matter if it targets player or not, both are moving at the same time lol
    /// </summary>
    public new bool targetPlayer => true;
    private int otherDir;
    // private double _clock = 0;
    // private bool pointAtPlayer;

    public override void Begin(G g, State s, Combat c)
    {
        bool debugMode = FeatureFlags.Debug && Input.shift;
        // _clock += g.dt / 2;
        // pointAtPlayer = (int)Math.Round(_clock) % 2 == 0;
        // if (s.route is not Combat)
        // {
        //     pointAtPlayer = true;
        // }

        otherDir = dir;
        if (c.otherShip.Get(Status.hermes) > 0 && !ignoreHermes)
        {
            if (otherDir == 0 && preferRightWhenZero)
            {
                otherDir += c.otherShip.Get(Status.hermes);
            }
            else
            {
                otherDir += (otherDir > 0) ? c.otherShip.Get(Status.hermes) : (c.otherShip.Get(Status.hermes) * -1);
            }
        }

        // Skip opponent move if opponent doesn't move
        if (otherDir == 0)
        {
            goto endOpponentMove;
        }

        // Random move on opponent
        if (isRandom && s.rngActions.Next() < 0.5)
        {
            otherDir *= -1;
        }

        // Checks if ship can move or not, also reduces engineStall if present
        if (PawsaiMovingHelper.CheckIfShipCantMove(c.otherShip, debugMode))
        {
            Audio.Play(Event.Status_PowerDown, true);
            c.otherShip.shake += 1.0;
            goto endOpponentMove;
        }

        if (c.otherShip.Get(Status.strafe) > 0)
        {
            c.QueueImmediate(new AAttack
            {
                damage = Card.GetActualDamage(s, c.otherShip.Get(Status.strafe), true, null),
                targetPlayer = true,
                fast = true
            });
        }

        if (isTeleport) PawsaiMovingHelper.DoTeleportationVisualMagic(g, s, c, c.otherShip, otherDir);

        c.otherShip.x += otherDir;
        if (PawsaiMovingHelper.CheckAgainstWall(g, s, c, c.otherShip, out GUID? guid))
        {
            Audio.Play(guid, true);
        }

        if (isTeleport) c.otherShip.xLerped = c.otherShip.x;

        Audio.Play(Event.Move, true);

        // Storyvar thing
        s.storyVars.shipsDontOverlapAtAll = s.ship.x >= c.otherShip.x + c.otherShip.parts.Count || s.ship.x + s.ship.parts.Count <= c.otherShip.x;

        endOpponentMove:
        base.Begin(g, s, c);
    }

    public override List<Tooltip> GetTooltips(State s)
    {
        if (isRandom)
        {
            return
            [
                new GlossaryTooltip("randomMirrorMovement")
                {
                    Title = ModEntry.Instance.Localizations.Localize(["Pawsai", "action", "MirrorMoveRandom", "title"]),
                    Icon = ModEntry.Instance.Action_MirrorMove_Random,
                    Description = ModEntry.Instance.Localizations.Localize(["Pawsai", "action", "MirrorMoveRandom", "desc"], new List<string>(){GetActualMovement(s).ToString()})
                }
                // new GlossaryTooltip("randomMirrorMovement")
                // {
                //     Title = ModEntry.Instance.Localizations.Localize(["Pawsai", "action", "MirrorMoveRight", "title"]),
                //     Icon = pointAtPlayer? ModEntry.Instance.Action_MirrorMove_Random : ModEntry.Instance.Action_MirrorMove_RandomFoe,
                //     Description = ModEntry.Instance.Localizations.Localize(["Pawsai", "action", "MirrorMoveRight", "desc"], new List<string>(){GetActualMovement(s, pointAtPlayer).ToString()})
                // }
            ];
        }

        return (dir, preferRightWhenZero) switch
        {
            (>0, _) or (0, true) => [new GlossaryTooltip("rightMirrorMovement")
                {
                    Title = ModEntry.Instance.Localizations.Localize(["Pawsai", "action", "MirrorMoveRight", "title"]),
                    Icon = ModEntry.Instance.Action_MirrorMove_Right,
                    Description = ModEntry.Instance.Localizations.Localize(["Pawsai", "action", "MirrorMoveRight", "desc"], new List<string>(){GetActualMovement(s).ToString()})
                }],
            (<0, _) => [new GlossaryTooltip("leftMirrorMovement")
                {
                    Title = ModEntry.Instance.Localizations.Localize(["Pawsai", "action", "MirrorMoveLeft", "title"]),
                    Icon = ModEntry.Instance.Action_MirrorMove_Left,
                    Description = ModEntry.Instance.Localizations.Localize(["Pawsai", "action", "MirrorMoveLeft", "desc"], new List<string>(){GetActualMovement(s).ToString()})
                }],
            (_, _) => [new GlossaryTooltip("zeroMirrorMovement")
                {
                    Title = ModEntry.Instance.Localizations.Localize(["Pawsai", "action", "MirrorMoveZero", "title"]),
                    Icon = ModEntry.Instance.Action_MirrorMove_Zero,
                    Description = ModEntry.Instance.Localizations.Localize(["Pawsai", "action", "MirrorMoveZero", "desc"], new List<string>(){GetActualMovement(s).ToString()})
                }]
        };
    }

    public override Icon? GetIcon(State s)
    {
        if (isRandom)
        {
            return new Icon
            (
                base.RandomMeansLeft(s) ? ModEntry.Instance.Action_MirrorMove_RandomLeft : ModEntry.Instance.Action_MirrorMove_Random,
                GetActualMovement(s),
                Colors.textMain,
                false
            );
        }
        if (GetActualMovement(s) > 0 || preferRightWhenZero)
        {
            return new Icon
            (
                ModEntry.Instance.Action_MirrorMove_Right,
                GetActualMovement(s),
                Colors.textMain,
                false
            );
        }
        return new Icon
        (
            ModEntry.Instance.Action_MirrorMove_RandomLeft,
            GetActualMovement(s),
            Colors.textMain,
            false
        );
    }

    private int GetActualMovement(State s, bool targetPlayer = true)
    {
        int n = dir;
        if (targetPlayer && s.ship.Get(Status.hermes) > 0)
        {
            n += dir > 0 ? s.ship.Get(Status.hermes) : s.ship.Get(Status.hermes) * -1;
        }
        else if (!targetPlayer)
        {
            try
            {
                if (s.route is Combat c && c.otherShip.Get(Status.hermes) > 0)
                {
                    n += dir > 0 ? c.otherShip.Get(Status.hermes) : c.otherShip.Get(Status.hermes) * -1;
                }
            }
            catch (Exception err)
            {
                ModEntry.Instance.Logger.LogError(err, "whoops mrirormoaewrfhwoae");
            }
        }
        return Math.Abs(n);
    }
}