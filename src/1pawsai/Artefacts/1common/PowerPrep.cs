namespace Starhunters.Pawsai.Artifacts;

[ArtifactMeta(pools = new[] { ArtifactPool.Common })]
public class PowerPrep : Artifact
{
    public override void OnTurnStart(State state, Combat combat)
    {
        if (combat.turn == 1)
        {
            combat.QueueImmediate(new AStatus
            {
                status = ModEntry.Instance.Status_Repetition.Status,
                statusAmount = 1,
                targetPlayer = true,
                artifactPulse = Key()
            });
        }
    }
}