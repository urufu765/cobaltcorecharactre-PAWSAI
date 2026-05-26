using System.Linq;

namespace Starhunters.Eriska.Artifacts;

[ArtifactMeta(pools = [ArtifactPool.Boss])]
public class ChemicalFire : Artifact
{
}


public static class ChemicalFireHelper
{
    public static bool enemyOverheating = false;
    public static bool playerOverheating = false;
    public static void CheckOverheat(AOverheat __instance, State s, Combat c) // prefix
    {
        if (__instance.targetPlayer)
        {
            enemyOverheating = false;
            playerOverheating = s.ship.Get(Status.heat) >= s.ship.heatTrigger;
        }
        else
        {
            if (c.otherShip is not null)
            {
                enemyOverheating = c.otherShip.Get(Status.heat) >= c.otherShip.heatTrigger;
            }
            else
            {
                enemyOverheating = false;
            }
            playerOverheating = false;
        }
    }
    public static void GiveBurnOnOverheat(AOverheat __instance, State s, Combat c)  // postfix
    {
        if (s.EnumerateAllArtifacts().Find(a => a is ChemicalFire) is ChemicalFire cf)
        {
            if (__instance.targetPlayer && playerOverheating)
            {
                s.ship.Add(ModEntry.Instance.BurnStatus.Status);
                cf.Pulse();
                playerOverheating = false;
            }
            else if (enemyOverheating && c.otherShip is not null)
            {
                c.otherShip.Add(ModEntry.Instance.BurnStatus.Status);
                cf.Pulse();
                enemyOverheating = false;
            }
        }
    }
}