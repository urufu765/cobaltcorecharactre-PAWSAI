using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Starhunters.Bruno.Artifacts;
using Starhunters.External;

namespace Starhunters.Bruno.Statuses;

public class Recoil : IKokoroApi.IV2.IStatusLogicApi.IHook
{
    public Recoil()
    {
        ModEntry.Instance.KokoroApi.V2.StatusLogic.RegisterHook(this);

        ModEntry.Instance.Harmony.Patch(
            original: AccessTools.DeclaredMethod(typeof(Card), nameof(Card.GetDmg)),
            postfix: new HarmonyMethod(MethodBase.GetCurrentMethod()!.DeclaringType!, nameof(FuckYourDamage))
        );
    }

    public static void FuckYourDamage(ref int __result, State s, bool targetPlayer = false)
    {
        if (targetPlayer)
        {
            if (s.route is Combat c && c is not null)
            {
                if (c.otherShip.Get(ModEntry.Instance.Status_Recoil.Status) > 0)
                {
                    __result = 0;
                }
                if (c.otherShip.Get(ModEntry.Instance.Status_Hamper.Status) > 0)
                {
                    __result = Math.Min(1, __result);
                }
            }
        }
        else
        {
            if (s.ship.Get(ModEntry.Instance.Status_Recoil.Status) > 0)
            {
                __result = 0;
            }
            if (s.ship.Get(ModEntry.Instance.Status_Hamper.Status) > 0)
            {
                __result = Math.Min(1, __result);
            }
        }
    }

    public bool HandleStatusTurnAutoStep(IKokoroApi.IV2.IStatusLogicApi.IHook.IHandleStatusTurnAutoStepArgs args)
    {
        if (args.Status != ModEntry.Instance.Status_Recoil.Status) return false;
        if (args.Timing != IKokoroApi.IV2.IStatusLogicApi.StatusTurnTriggerTiming.TurnStart) return false;
        if (args.Amount > 0)
        {
            args.Amount = 0;
        }
        return false;
    }

    public int ModifyStatusChange(IKokoroApi.IV2.IStatusLogicApi.IHook.IModifyStatusChangeArgs args)
    {
        if (args.Status == ModEntry.Instance.Status_Recoil.Status && args.NewAmount > 0 && args.State.EnumerateAllArtifacts().Any(a => a is ThermoelectricCannonCooler))
        {
            args.Ship.Add(ModEntry.Instance.Status_Hamper.Status, args.NewAmount);
            return 0;
        }
        return args.NewAmount;
    }
}