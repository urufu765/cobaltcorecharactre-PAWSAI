using System;
using System.Collections.Generic;

namespace Starhunters.Pawsai.Artifacts;

[ArtifactMeta(pools = new[] { ArtifactPool.Boss })]
public class PainCruiser : Artifact
{
    public int RawShield { get; set; }
    public override void OnPlayerTakeNormalDamage(State state, Combat combat, int rawAmount, Part? part)
    {
        RawShield = state.ship.Get(Status.tempShield) + state.ship.Get(Status.shield);
    }
}

public static class PainCruiserHelper
{
    public static void HopeYouLikePain(Ship __instance, State s, Combat c, int amt)
    {
        if (__instance.isPlayerShip && s.EnumerateAllArtifacts().Find(a => a is PainCruiser) is PainCruiser pc)
        {
            int nowShield = __instance.Get(Status.tempShield) + __instance.Get(Status.shield);
            int nowDamage = Math.Max(0, pc.RawShield - nowShield);
            if (__instance.Get(Status.perfectShield) == 0)
            {
                nowDamage += amt;
            }

            if (nowDamage > 3)
            {
                c.QueueImmediate(new AStatus
                {
                    status = Status.payback,
                    statusAmount = 1,
                    targetPlayer = true,
                    artifactPulse = pc.Key()
                });
            }
            pc.RawShield = 0;
        }
    }
}