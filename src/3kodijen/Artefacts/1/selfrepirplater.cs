using System.Collections.Generic;

namespace Starhunters.Kodijen.Artifacts;

[ArtifactMeta(pools = new[] { ArtifactPool.Common })]
public class SelfRepairingPlaster : Artifact
{
    public override void OnCombatEnd(State state)
    {
        if (state.ship.Get(ModEntry.Instance.Status_HullCapacity.Status) > 4)
        {
            state.rewardsQueue.QueueImmediate(
                new AHeal
                {
                    healAmount = 1,
                    targetPlayer = true,
                    artifactPulse = Key()
                }
            );
        }
    }

    public override List<Tooltip>? GetExtraTooltips()
    {
        List<Tooltip> l = StatusMeta.GetTooltips(ModEntry.Instance.Status_HullCapacity.Status, 1);
        return l;
    }
}