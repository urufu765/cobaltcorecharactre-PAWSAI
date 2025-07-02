using System.Collections.Generic;

namespace Starhunters.Bruno.Artifacts;

[ArtifactMeta(pools = new[] { ArtifactPool.Common })]
public class ThermoelectricCannonCooler : Artifact
{
    public override List<Tooltip>? GetExtraTooltips()
    {
        List<Tooltip> l = StatusMeta.GetTooltips(ModEntry.Instance.Status_Recoil.Status, 1);
        l.AddRange(StatusMeta.GetTooltips(ModEntry.Instance.Status_Hamper.Status, 1));
        return l;
    }
}