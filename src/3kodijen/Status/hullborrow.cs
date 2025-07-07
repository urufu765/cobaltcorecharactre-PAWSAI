using Starhunters.External;
using Starhunters.Kodijen.Actions;

namespace Starhunters.Kodijen.Statuses;

public class BorrowedHull : IKokoroApi.IV2.IStatusLogicApi.IHook
{
    public BorrowedHull()
    {
        ModEntry.Instance.KokoroApi.V2.StatusLogic.RegisterHook(this);
    }

    public bool? IsAffectedByBoost(IKokoroApi.IV2.IStatusLogicApi.IHook.IIsAffectedByBoostArgs args)
    {
        if (args.Status == ModEntry.Instance.Status_BorrowedHull.Status)
        {
            return false;
        }
        return null;
    }

    public int ModifyStatusChange(IKokoroApi.IV2.IStatusLogicApi.IHook.IModifyStatusChangeArgs args)
    {
        if (args.Status == ModEntry.Instance.Status_BorrowedHull.Status)
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
                args.State.rewardsQueue.Queue(new AShipUpgrades
                {
                    actions =
                    {
                        new AHullMax
                        {
                            amount = args.OldAmount,
                            targetPlayer = true
                        },
                        new AHeal
                        {
                            healAmount = args.OldAmount,
                            targetPlayer = true,
                        }
                    }
                });
                return 0;
            }

            // Safety check should be done when drone launches anyways. Also lets you launch drones with hull borrow even if you don't have enough hull (as long as you have at least one to give)... they just start with reduced hull
            if (args.NewAmount > 0 && args.Ship.hull > 1 && args.Ship.hullMax > args.NewAmount)
            {
                args.Combat.QueueImmediate(new AHullChange
                {
                    amount = -args.NewAmount,
                    targetPlayer = isPlayer
                });
            }

            // You can't just un-borrow hull! That's illegal!
            else if (args.NewAmount < 0)
            {
                return 0;
            }
        }

        return args.NewAmount;
    }
}

public static class HullRestoration
{
    public static void RestoreHullTheWayItUsedToBe(Ship __instance)
    {
        if (__instance.Get(ModEntry.Instance.Status_BorrowedHull.Status) > 0)
        {
            __instance.Set(ModEntry.Instance.Status_BorrowedHull.Status, 0);
        }
        if (__instance.Get(ModEntry.Instance.Status_HullCapacity.Status) > 0)
        {
            __instance.Set(ModEntry.Instance.Status_HullCapacity.Status, 0);
        }
    }
}