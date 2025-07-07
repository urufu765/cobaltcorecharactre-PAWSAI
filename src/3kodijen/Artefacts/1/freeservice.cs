using System.Collections.Generic;

namespace Starhunters.Kodijen.Artifacts;

[ArtifactMeta(pools = new[] { ArtifactPool.Common })]
public class FirstOnesFree : Artifact
{
    // Do the rest of the function in the spawn thing thing

    public override List<Tooltip>? GetExtraTooltips()
    {
        List<Tooltip> l = StatusMeta.GetTooltips(ModEntry.Instance.Status_HullCapacity.Status, 1);
        return l;
    }
}