using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Extensions.Logging;
using Nanoray.Shrike;
using Nanoray.Shrike.Harmony;
using Starhunters.External;

namespace Starhunters.Pawsai.Statuses;

public class Repulsion : IKokoroApi.IV2.IStatusLogicApi.IHook
{
    private static int playerLastShield = 0;
    private static int enemyLastShield = 0;
    public Repulsion()
    {
        ModEntry.Instance.KokoroApi.V2.StatusLogic.RegisterHook(this);

        ModEntry.Instance.Harmony.Patch(
            original: AccessTools.DeclaredMethod(typeof(AAttack), nameof(AAttack.GetFromX)),
            postfix: new HarmonyMethod(MethodBase.GetCurrentMethod()!.DeclaringType!, nameof(FukinRepulsedByThatAttack))
        );
        ModEntry.Instance.Harmony.Patch(
            original: AccessTools.DeclaredMethod(typeof(AAttack), nameof(AAttack.Begin)),
            transpiler: new HarmonyMethod(MethodBase.GetCurrentMethod()!.DeclaringType!, nameof(SetUpRepulsionShots))
        );
    }

    private static IEnumerable<CodeInstruction> SetUpRepulsionShots(IEnumerable<CodeInstruction> instructions, ILGenerator il)
    {
        try
        {
            return new SequenceBlockMatcher<CodeInstruction>(instructions)
                .Find(
                    SequenceBlockMatcherFindOccurence.First,
                    SequenceMatcherRelativeBounds.WholeSequence,
                    ILMatches.AnyLdloc,
                    ILMatches.AnyLdloc.CreateLdlocInstruction(out var instr),
                    ILMatches.Ldfld(AccessTools.DeclaredField(typeof(RaycastResult), "worldX"))
                )
                .Insert(
                    SequenceMatcherPastBoundsDirection.Before,
                    SequenceMatcherInsertionResultingBounds.JustInsertion,
                    [
                        new(OpCodes.Ldarg_0),
                        new(OpCodes.Ldarg_2),
                        new(OpCodes.Ldarg_3),
                        new(instr.Value),
                        new(OpCodes.Call, AccessTools.DeclaredMethod(typeof(Repulsion), nameof(CheckThenBounce)))
                    ]
                ).AllElements();
        }
        catch (Exception err)
        {
            ModEntry.Instance.Logger.LogError(err, "Oh noes!!!");
            throw;
        }
    }

    private static void CheckThenBounce(AAttack instance, State s, Combat c, RaycastResult rcr)
    {
        Ship ship = instance.targetPlayer ? s.ship : c.otherShip;
        if (!instance.isBeam && ship.Get(ModEntry.Instance.Status_Repulsion.Status) > 0 && (instance.targetPlayer ? playerLastShield : enemyLastShield) > 0 && instance.paybackCounter < 100)
        {
            AAttack aAttack = new AAttack
            {
                paybackCounter = instance.paybackCounter + 1,
                damage = Card.GetActualDamage(s, ship.Get(ModEntry.Instance.Status_Repulsion.Status), !instance.targetPlayer),
                targetPlayer = !instance.targetPlayer,
                fast = true
            };
            int localX = rcr.worldX - ship.x;
            ModEntry.Instance.Helper.ModData.SetModData(aAttack, "repulseAttack", localX);
            c.QueueImmediate(aAttack);
        }
    }

    private static void FukinRepulsedByThatAttack(AAttack __instance, ref int? __result, State s, Combat c)
    {
        playerLastShield = s.ship.Get(Status.shield);
        enemyLastShield = c.otherShip.Get(Status.shield);

        if (ModEntry.Instance.Helper.ModData.TryGetModData(__instance, "repulseAttack", out int repulsed))
        {
            __result = repulsed;
        }
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