using System.Linq;
using Starhunters.Bruno.Artifacts;
using Starhunters.External;

namespace Starhunters.Bruno.Statuses;

public class SlowBurn : IKokoroApi.IV2.IStatusLogicApi.IHook
{
    public SlowBurn()
    {
        ModEntry.Instance.KokoroApi.V2.StatusLogic.RegisterHook(this);
    }

    public bool HandleStatusTurnAutoStep(IKokoroApi.IV2.IStatusLogicApi.IHook.IHandleStatusTurnAutoStepArgs args)
    {
        if (args.Status != ModEntry.Instance.Status_SlowBurn.Status) return false;
        if (args.Timing != IKokoroApi.IV2.IStatusLogicApi.StatusTurnTriggerTiming.TurnStart) return false;
        if (args.Amount > 0)
        {
            args.Amount -= 1;
            args.Combat.QueueImmediate(new AHurt
            {
                hurtAmount = 1,
                targetPlayer = true,
                hurtShieldsFirst = true
            });
        }
        return false;
    }
}