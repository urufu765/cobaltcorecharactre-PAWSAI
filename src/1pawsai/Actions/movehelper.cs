using System;
using System.Collections.Generic;
using System.Linq;
using FMOD;
using FSPRO;
using Starhunters.Pawsai.Artifacts;

namespace Starhunters.Pawsai.Actions;

public static class PawsaiMovingHelper
{
    public static void FlipThemActionsOverriden(Card __instance, ref List<CardAction> __result, State s)
    {
        // Flip them actions
        if (__instance.flipped)
        {
            foreach (CardAction cardAction in __result)
            {
                if (cardAction is APivot ap)
                {
                    ap.outward = !ap.outward;
                }
                if (cardAction is AMirrorMove am)
                {
                    am.dir *= -1;
                    am.preferRightWhenZero = !am.preferRightWhenZero;
                }
            }
        }

        // OffensiveDefense
        if (__instance.GetMeta().deck == ModEntry.Instance.PawsaiDeck.Deck && s.EnumerateAllArtifacts().Find(a => a is OffensiveDefense) is OffensiveDefense od)
        {
            AAttack aAttack = new AAttack
            {
                damage = __instance.GetDataWithOverrides(s).cost,
                fast = true,
                artifactPulse = od.Key()
            };
            ModEntry.Instance.Helper.ModData.SetModData(aAttack, "noTouch", true);
            __result.Insert(0, aAttack);
        }
    }

    /// <summary>
    /// Teleportation shit
    /// </summary>
    /// <param name="g"></param>
    /// <param name="s"></param>
    /// <param name="c"></param>
    /// <param name="theShip"></param>
    /// <param name="dir"></param>
    public static void DoTeleportationVisualMagic(G g, State s, Combat c, Ship theShip, int dir)
    {
        for (int i = 0; i < theShip.parts.Count * 5; i++)
        {
            PFX.combatAdd.Add(new Particle
            {
                pos = theShip.GetWorldPos(g.state, c) + new Vec(Mutil.NextRand() * theShip.parts.Count * 16, (theShip == s.ship ? 1 : -1) * (-10 + Mutil.NextRand() * 20)),
                vel = Mutil.RandVel() * 30.0 + new Vec(dir * 20, 0),
                color = new Color(1, 0.3, 1).gain(0.5),
                size = 1 + Mutil.NextRand(),
                dragCoef = 4 + Mutil.NextRand(),
                lifetime = 0.2 + Mutil.NextRand()
            });
        }
    }

    /// <summary>
    /// Check ship against walls, do damage if necessary.
    /// </summary>
    /// <param name="g"></param>
    /// <param name="s"></param>
    /// <param name="c"></param>
    /// <param name="theShip"></param>
    /// <returns></returns>
    public static bool CheckAgainstWall(G g, State s, Combat c, Ship theShip, out GUID? guid)
    {
        int checkWall = theShip.x;
        int particleBuster = 0;
        int? particlePoint = null;
        if (c.leftWall is int l) checkWall = Math.Max(l, checkWall);
        if (c.rightWall is int r) checkWall = Math.Max(r - theShip.parts.Count, checkWall);
        switch (theShip.x)
        {
            case int a when a < checkWall:
                theShip.xLerped = checkWall - 0.25;
                theShip.x = checkWall;
                particleBuster = -1;
                particlePoint = theShip.x;
                break;
            case int a when a > checkWall:
                theShip.xLerped = checkWall + 0.25;
                theShip.x = checkWall;
                particleBuster = 1;
                particlePoint = theShip.x + theShip.parts.Count - 1;
                break;
        }

        if (particlePoint is int p && c.wallDamage is int wd && wd > 0)
        {
            DamageDone dmgDone = theShip.NormalDamage(s, c, wd, p, false);
            guid = Event.Hits_HitHurt;
            if (dmgDone.hitShield) guid = Event.Hits_ShieldHit;
            if (dmgDone.poppedShield) guid = Event.Hits_ShieldPop;
            if (dmgDone.hitHull) guid = Event.Hits_HitHurt;
            Audio.Play(guid, true);
            ParticleBursts.WallImpact
            (
                g,
                FxPositions.WallImpact(p, theShip == s.ship) + new Vec(particleBuster * 8, 0),
                (dmgDone.hitShield || dmgDone.poppedShield) && dmgDone.hullAmt == 0,
                -particleBuster
            );
            return true;
        }

        guid = null;
        return false;
    }

    /// <summary>
    /// Checks if ship is able to move at all. Doesn't account for anchor
    /// </summary>
    /// <param name="theShip">Target ship</param>
    /// <param name="debugMode">Ignore move restriction</param>
    /// <param name="tickEngineStall">Reduce engine stall stack</param>
    /// <returns></returns>
    public static bool CheckIfShipCantMove(Ship theShip, bool debugMode = false, bool tickEngineStall = true)
    {
        if (debugMode) return false;

        if (theShip.Get(Status.lockdown) > 0 || theShip.immovable)
        {
            return true;
        }

        if (theShip.Get(Status.engineStall) > 0)
        {
            if (tickEngineStall) theShip.Add(Status.engineStall, -1);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Checks if ship is able to move at all.
    /// </summary>
    /// <param name="state"></param>
    /// <param name="combat"></param>
    /// <param name="theShip">Target ship</param>
    /// <param name="debugMode">Ignore move restriction</param>
    /// <param name="tickEngineStall">Reduce engine stall stack</param>
    /// <returns></returns>
    public static bool CheckIfShipCantMove(State state, Combat combat, Ship theShip, bool debugMode = false, bool tickEngineStall = true)
    {
        if (debugMode) return false;

        if (theShip == state.ship && combat.hand.OfType<TrashAnchor>().Any<TrashAnchor>())
        {
            return true;
        }

        if (theShip.Get(Status.lockdown) > 0 || theShip.immovable)
        {
            return true;
        }

        if (theShip.Get(Status.engineStall) > 0)
        {
            if (tickEngineStall) theShip.Add(Status.engineStall, -1);
            return true;
        }
        return false;
    }
}