using System.Collections.Generic;

namespace Starhunters.Bruno.Artifacts;

[ArtifactMeta(pools = new[] { ArtifactPool.Boss })]
public class ForsakenSafety : Artifact
{
    public override List<Tooltip>? GetExtraTooltips()
    {
        List<Tooltip> l = StatusMeta.GetTooltips(ModEntry.Instance.Status_Mitigate.Status, 1);
        l.AddRange(StatusMeta.GetTooltips(ModEntry.Instance.KokoroApi.V2.DriveStatus.Pulsedrive, 1));
        return l;
    }
}