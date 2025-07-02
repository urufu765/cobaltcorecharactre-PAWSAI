using System.Linq;

namespace Starhunters.Bruno.Artifacts;

[ArtifactMeta(pools = new[] { ArtifactPool.Boss })]
public class DoubleEdgedCannon : Artifact
{
}


/// <summary>
/// Also ends up helping for UnfairAdvantage
/// </summary>
public static class DoubleEdgedCannonHelper
{
    public static void ItWillHurtTwiceAsMuch(Ship __instance, State s, Combat c, ref int amt)
    {
        if (s.EnumerateAllArtifacts().Any(a => a is DoubleEdgedCannon))
        {
            amt *= 2;
        }

        if (
            !__instance.isPlayerShip &&
            c.otherShip.Get(Status.shield) <= 0 &&
            s.EnumerateAllArtifacts().Find(a => a is UnfairAdvantage) is UnfairAdvantage ua &&
            ua.AdvantageActive
        )
        {
            ua.AdvantageActive = false;
            c.QueueImmediate(new AStunShip
            {
                artifactPulse = ua.Key()
            });
        }
    }
}