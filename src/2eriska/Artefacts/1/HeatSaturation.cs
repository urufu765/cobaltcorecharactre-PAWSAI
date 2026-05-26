namespace Starhunters.Eriska.Artifacts;

[ArtifactMeta(pools = [ArtifactPool.Common])]
public class HeatSaturation : Artifact
{
    public bool Depleted { get; set; } = false;

    public override void OnCombatEnd(State state)
    {
        Depleted = false;
    }

    public override void OnTurnStart(State state, Combat combat)
    {
        Depleted = false;
    }

    public override Spr GetSprite()
    {
        return Depleted ? ModEntry.Instance.SprArtHeatSaturationDepleted : base.GetSprite();
    }
}


public static class HeatSaturationHelper
{
    public static void HeatReducer(ref AStatus __instance, State s)  // prefix
    {
        if (__instance.status == Status.heat && __instance.statusAmount > 0 && s.EnumerateAllArtifacts().Find(a => a is HeatSaturation) is HeatSaturation hs && !hs.Depleted)
        {
            __instance.statusAmount--;
            hs.Depleted = true;
            hs.Pulse();
        }
    }
}