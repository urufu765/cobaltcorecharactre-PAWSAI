using Starhunters.External;
using Starhunters.Kodijen.Actions;

namespace Starhunters.Kodijen.Statuses;

public class HullCapacity : IKokoroApi.IV2.IStatusLogicApi.IHook
{
    public HullCapacity()
    {
        ModEntry.Instance.KokoroApi.V2.StatusLogic.RegisterHook(this);
    }

    public int ModifyStatusChange(IKokoroApi.IV2.IStatusLogicApi.IHook.IModifyStatusChangeArgs args)
    {
        if (args.Status == ModEntry.Instance.Status_HullCapacity.Status)
        {
            bool isPlayer = args.Ship == args.State.ship;
            // Restore hull the way it should be at the end of combat
            if (
                isPlayer &&
                args.Combat.hand.Count == 0 &&
                args.Combat.exhausted.Count == 0 &&
                args.Combat.exhausted.Count == 0
            )
            {
                args.State.rewardsQueue.Queue(new AHullChange
                {
                    amount = -args.OldAmount,
                    targetPlayer = true
                });
                return 0;
            }

            if (args.NewAmount > 0)  // Gain hull capacity!
            {
                args.Combat.QueueImmediate([
                    new AHullMax
                    {
                        amount = args.NewAmount,
                        targetPlayer = isPlayer
                    },
                    new AHeal
                    {
                        healAmount = args.NewAmount,
                        targetPlayer = isPlayer,
                    }
                ]);
            }
            else if (args.NewAmount < 0 && args.OldAmount > 0)  // Lose hull capacity!
            {
                args.Combat.QueueImmediate(new AHullChange
                {
                    amount = (args.NewAmount * -1 >= args.OldAmount)? args.NewAmount : -args.OldAmount,
                    targetPlayer = isPlayer
                });
            }
        }

        return args.NewAmount;
    }
}