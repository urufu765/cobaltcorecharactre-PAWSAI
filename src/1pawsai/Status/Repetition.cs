using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Starhunters.External;

namespace Starhunters.Pawsai.Features;

public class Repetition : IKokoroApi.IV2.IStatusLogicApi.IHook
{
    public Repetition()
    {
        ModEntry.Instance.KokoroApi.V2.StatusLogic.RegisterHook(this);

        ModEntry.Instance.Harmony.Patch(
            original: AccessTools.DeclaredMethod(typeof(Combat), nameof(Combat.TryPlayCard)),
            postfix: new HarmonyMethod(MethodBase.GetCurrentMethod()!.DeclaringType!, nameof(LetsSeeThatAgain))
        );
    }

    private void LetsSeeThatAgain(bool __result, Combat __instance, State s, Card card)
    {
        if (!__result) return;
        if (s.ship.Get(ModEntry.Instance.Status_Repetition.Status) > 0)
        {
            List<CardAction> actionz = card.GetActionsOverridden(s, __instance);
            foreach (CardAction action in actionz)
            {
                action.whoDidThis = card.GetMeta().deck;
            }
            __instance.Queue(actionz);
            s.ship.Add(ModEntry.Instance.Status_Repetition.Status, -1);
        }
    }

    public bool HandleStatusTurnAutoStep(IKokoroApi.IV2.IStatusLogicApi.IHook.IHandleStatusTurnAutoStepArgs args)
    {
        if (args.Status != ModEntry.Instance.Status_Repetition.Status) return false;
        if (args.Timing != IKokoroApi.IV2.IStatusLogicApi.StatusTurnTriggerTiming.TurnStart) return false;
        if (args.Amount > 0)
        {
            args.Amount -= 1;
        }
        return false;
    }
}