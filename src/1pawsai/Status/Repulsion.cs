using System;
using System.Reflection;
using HarmonyLib;
using Starhunters.External;

namespace Starhunters.Pawsai.Features;

public class Repulsion : IKokoroApi.IV2.IStatusLogicApi.IHook
{
    public Repulsion()
    {
        ModEntry.Instance.KokoroApi.V2.StatusLogic.RegisterHook(this);

        ModEntry.Instance.Harmony.Patch(
            original: AccessTools.DeclaredMethod(typeof(Ship), nameof(Ship.ModifyDamageDueToParts)),
            postfix: new HarmonyMethod(MethodBase.GetCurrentMethod()!.DeclaringType!, nameof(FukinRepulsedByThatAttack))
        );
    }

    // TODO
    private void FukinRepulsedByThatAttack()
    {
        throw new NotImplementedException();
    }

    public bool HandleStatusTurnAutoStep(IKokoroApi.IV2.IStatusLogicApi.IHook.IHandleStatusTurnAutoStepArgs args)
    {
        if (args.Status != ModEntry.Instance.Status_Repulsion.Status) return false;
        if (args.Timing != IKokoroApi.IV2.IStatusLogicApi.StatusTurnTriggerTiming.TurnStart) return false;
        if (args.Amount > 0)
        {
            args.Amount -= 1;
        }
        return false;
    }
}