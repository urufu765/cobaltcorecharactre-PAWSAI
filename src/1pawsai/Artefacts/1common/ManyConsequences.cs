namespace Starhunters.Pawsai.Artifacts;

[ArtifactMeta(pools = new[] { ArtifactPool.Common }, unremovable = true)]
public class ManyConsequences : Artifact
{
    public override void OnReceiveArtifact(State state)
    {
        state.ship.shieldMaxBase += 5;
    }

    public override void OnCombatStart(State state, Combat combat)
    {
        if (state.ship.hull > state.ship.GetMaxShield())
        {
            combat.Queue(new AHurt
            {
                hurtAmount = 1,
                targetPlayer = true,
                artifactPulse = Key()
            });
        }
    }
}