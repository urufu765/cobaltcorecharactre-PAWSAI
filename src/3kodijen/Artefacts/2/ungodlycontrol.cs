using System.Collections.Generic;

namespace Starhunters.Kodijen.Artifacts;

[ArtifactMeta(pools = new[] { ArtifactPool.Common })]
public class UngodlyAmountOfControl : Artifact
{
    public bool Failed { get; set; } = false;

    public override void OnTurnStart(State state, Combat combat)
    {
        combat.Queue([
            new AStatus{
                status = Status.droneShift,
                statusAmount = 2,
                targetPlayer = true,
                artifactPulse = Key()
            },
            new AStatus{
                status = Status.engineStall,
                statusAmount = Failed? 3 : 1,
                targetPlayer = true,
                artifactPulse = Key()
            },
        ]);
    }

    public override void OnTurnEnd(State state, Combat combat)
    {
        Failed = state.ship.Get(Status.droneShift) > 0;
    }

    public override Spr GetSprite()
    {
        return Failed ? ModEntry.Instance.UngodlyAmountOfControl_Fail : base.GetSprite();
    }

    public override List<Tooltip>? GetExtraTooltips()
    {
        return [
            new TTGlossary("status.droneShift", []),
            new TTGlossary("status.engineStall", [Failed ? 3 : 1])
        ];
    }
}