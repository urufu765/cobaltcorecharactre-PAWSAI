using Starhunters.External;

namespace Starhunters.Pawsai.Statuses;

public class ShieldRegen : IKokoroApi.IV2.IStatusLogicApi.IHook
{
    public ShieldRegen()
    {
        ModEntry.Instance.KokoroApi.V2.StatusLogic.RegisterHook(this);
    }

    public bool HandleStatusTurnAutoStep(IKokoroApi.IV2.IStatusLogicApi.IHook.IHandleStatusTurnAutoStepArgs args)
    {
        if (args.Status != ModEntry.Instance.Status_Regeneration.Status) return false;
        if (args.Timing != IKokoroApi.IV2.IStatusLogicApi.StatusTurnTriggerTiming.TurnStart) return false;
        if (args.Amount > 0)
        {
            args.Amount -= 1;
        }
        return false;
    }

    public void OnStatusTurnTrigger(IKokoroApi.IV2.IStatusLogicApi.IHook.IOnStatusTurnTriggerArgs args)
    {
        if (args.Status != ModEntry.Instance.Status_Regeneration.Status) return;
        if (args.Timing != IKokoroApi.IV2.IStatusLogicApi.StatusTurnTriggerTiming.TurnStart) return;
        if (args.Ship.Get(ModEntry.Instance.Status_Regeneration.Status) > 0)
        {
            args.Combat.QueueImmediate(new AStatus
            {
                status = Status.shield,
                statusAmount = 1,
                targetPlayer = args.Ship == args.State.ship
            });
        }
    }
}