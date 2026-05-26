namespace Starhunters.Eriska.Artifacts;

[ArtifactMeta(pools = [ArtifactPool.Boss])]
public class Pyrotactics : Artifact
{
    public override void AfterPlayerOverheat(State state, Combat combat)
    {
        combat.QueueImmediate(new AHullMax
        {
            amount = -1,
            targetPlayer = true,
            artifactPulse = Key()
        });
    }

    public override void OnCombatStart(State state, Combat combat)
    {
        combat.otherShip.heatTrigger += 2;
    }

    public override void OnReceiveArtifact(State state)
    {
        state.ship.heatTrigger += 2;
    }

    public override void OnRemoveArtifact(State state)
    {
        state.ship.heatTrigger -= 2;
    }
}