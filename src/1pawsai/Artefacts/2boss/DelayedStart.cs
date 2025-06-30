using System.Collections.Generic;

namespace Starhunters.Pawsai.Artifacts;

[ArtifactMeta(pools = new[] { ArtifactPool.Boss })]
public class DelayedStart : Artifact
{
    public override void OnTurnEnd(State state, Combat combat)
    {
        if (combat.turn == 1)
        {
            foreach (Status stats in state.ship.statusEffects.Keys)
            {
                combat.Queue(new AStatus
                {
                    status = stats,
                    statusAmount = 2,
                    mode = AStatusMode.Mult,
                    targetPlayer = true,
                    artifactPulse = Key()
                });
            }
        }
    }
}