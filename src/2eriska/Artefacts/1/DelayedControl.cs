using System;
using System.Collections.Generic;

namespace Starhunters.Eriska.Artifacts;

[ArtifactMeta(pools = [ArtifactPool.Common])]
public class DelayedControlAid : Artifact
{
    public const int TURN_COUNT = 10;
    public override void OnTurnStart(State state, Combat combat)
    {
        if (combat.turn == TURN_COUNT)
        {
            combat.QueueImmediate(new AStatus
            {
                status = Status.tableFlip,
                statusAmount = 1,
                targetPlayer = true,
                artifactPulse = Key()
            });
        }
    }

    public override int? GetDisplayNumber(State s)
    {
        if (s.route is Combat c)
        {
            return Math.Max(0, TURN_COUNT - c.turn);
        }
        return base.GetDisplayNumber(s);
    }

    public override List<Tooltip>? GetExtraTooltips()
    {
        return [
            new TTGlossary("status.tableFlip")
        ];
    }
}