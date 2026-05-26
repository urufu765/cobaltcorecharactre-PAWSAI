using Starhunters.Eriska.API;

namespace Starhunters.Eriska.Artifacts;

[ArtifactMeta(pools = [ArtifactPool.Boss])]
public class Pyroforger : Artifact, IArtifactModifyBurnBlisterBaseDamage
{
    public bool Depleted { get; set; } = false;
    public override void AfterPlayerOverheat(State state, Combat combat)
    {
        if (!Depleted)
        {
            combat.QueueImmediate(new AHullMax
            {
                amount = 2,
                targetPlayer = true,
                artifactPulse = Key()
            });
            Depleted = true;
        }
    }

    public override void OnCombatStart(State state, Combat combat)
    {
        Depleted = false;
    }

    public override void OnCombatEnd(State state)
    {
        Depleted = false;
    }

    public override Spr GetSprite()
    {
        return Depleted ? ModEntry.Instance.SprArtPyroforgerDepleted : base.GetSprite();
    }

    public int ModifyBurnBaseDamage(State state, Combat combat, bool targetPlayer)
    {
        if (!targetPlayer)
        {
            return -1;
        }
        return 0;
    }

    public int ModifyBlisterBaseDamage(State state, Combat combat, bool targetPlayer)
    {
        if (!targetPlayer)
        {
            return -1;
        }
        return 0;
    }
}