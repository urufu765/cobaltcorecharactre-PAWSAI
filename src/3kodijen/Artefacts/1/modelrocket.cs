using System.Collections.Generic;

namespace Starhunters.Kodijen.Artifacts;

[ArtifactMeta(pools = new[] { ArtifactPool.Boss })]
public class ModelRocketDegree : Artifact
{
    public override int ModifyBaseDamage(int baseDamage, Card? card, State state, Combat? combat, bool fromPlayer)
    {
        if (fromPlayer) return -1;
        return 0;
    }

    public override int ModifyBaseMissileDamage(State state, Combat? combat, bool targetPlayer)
    {
        if (!targetPlayer) return 2;
        return 0;
    }

    public override List<Tooltip>? GetExtraTooltips()
    {
        List<Tooltip> l = StatusMeta.GetTooltips(ModEntry.Instance.Status_HullCapacity.Status, 1);
        return l;
    }
}