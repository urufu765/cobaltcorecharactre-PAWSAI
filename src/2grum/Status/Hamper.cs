using System.Linq;
using Starhunters.Bruno.Artifacts;
using Starhunters.External;

namespace Starhunters.Bruno.Statuses;

public class Hamper : IKokoroApi.IV2.IStatusLogicApi.IHook
{
    public Hamper()
    {
        ModEntry.Instance.KokoroApi.V2.StatusLogic.RegisterHook(this);
    }

    public bool HandleStatusTurnAutoStep(IKokoroApi.IV2.IStatusLogicApi.IHook.IHandleStatusTurnAutoStepArgs args)
    {
        if (args.Status != ModEntry.Instance.Status_Hamper.Status) return false;
        if (args.Timing != IKokoroApi.IV2.IStatusLogicApi.StatusTurnTriggerTiming.TurnStart) return false;
        if (args.Amount > 0)
        {
            args.Amount = 0;
        }
        return false;
    }
}