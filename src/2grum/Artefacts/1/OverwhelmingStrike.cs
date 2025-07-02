using System;
using System.Linq;

namespace Starhunters.Bruno.Artifacts;

[ArtifactMeta(pools = new[] { ArtifactPool.Boss })]
public class OverwhelmingStrike : Artifact
{
}

public static class OverwhelmingStrikeHelper
{
    private static bool overwhelmingStrikeReady;

    /// <summary>
    /// Since for some reason they update the incomingDamage in the original code. Doesn't touch the values in prefix just in case someone else also has this problem.
    /// </summary>
    /// <param name="__instance"></param>
    /// <param name="incomingDamage"></param>
    public static void CheckIncomingDamage(Ship __instance, int incomingDamage)
    {
        overwhelmingStrikeReady = !__instance.isPlayerShip && incomingDamage >= 5;
    }

    public static void IgnoreArmorLol(Ship __instance, ref int __result, State s, Part part, bool piercing = false)
    {
        if (
            !__instance.isPlayerShip &&
            !piercing &&
            overwhelmingStrikeReady &&
            part.GetDamageModifier() == PDamMod.armor &&
            !part.invincible &&
            s.EnumerateAllArtifacts().Any(a => a is OverwhelmingStrike)
        )
        {
            __result += 1;
            s.storyVars.damageBlockedByEnemyArmorThisTurn--;
        }
    }

}