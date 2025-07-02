using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Extensions.Logging;
using Nanoray.Shrike;
using Nanoray.Shrike.Harmony;
using Starhunters.Bruno.Artifacts;
using Starhunters.External;

namespace Starhunters.Bruno.Statuses;

public class Mitigate : IKokoroApi.IV2.IStatusLogicApi.IHook
{
    public Mitigate()
    {
        ModEntry.Instance.KokoroApi.V2.StatusLogic.RegisterHook(this);

        ModEntry.Instance.Harmony.Patch(
            original: AccessTools.DeclaredMethod(typeof(Ship), nameof(Ship.NormalDamage)),
            transpiler: new HarmonyMethod(MethodBase.GetCurrentMethod()!.DeclaringType!, nameof(DamageMitigation))
        );
    }

    private static IEnumerable<CodeInstruction> DamageMitigation(IEnumerable<CodeInstruction> instructions, ILGenerator il)
    {
        try
        {
            return new SequenceBlockMatcher<CodeInstruction>(instructions)
                .Find(
                    SequenceBlockMatcherFindOccurence.First,
                    SequenceMatcherRelativeBounds.WholeSequence,
                    ILMatches.AnyLdloc.GetLocalIndex(out var instr),
                    ILMatches.AnyStloc,
                    ILMatches.Ldfld(AccessTools.DeclaredField(typeof(RaycastResult), "worldX"))
                )
                .Insert(
                    SequenceMatcherPastBoundsDirection.Before,
                    SequenceMatcherInsertionResultingBounds.JustInsertion,
                    [
                        new(OpCodes.Ldarg_0),
                        new(OpCodes.Ldloc, instr.Value),
                        new(OpCodes.Call, AccessTools.DeclaredMethod(typeof(Mitigate), nameof(MitigateSomeDamage))),
                        new(OpCodes.Stloc, instr.Value),
                    ]
                ).AllElements();
        }
        catch (Exception err)
        {
            ModEntry.Instance.Logger.LogError(err, "Oh noes!!!");
            throw;
        }
    }

    private static int MitigateSomeDamage(Ship ship, int incomingDamage)
    {
        if (ship.Get(ModEntry.Instance.Status_Mitigate.Status) > 0)
        {
            int mitigateAmount = Math.Min(ship.Get(ModEntry.Instance.Status_Mitigate.Status), incomingDamage);
            ship.Add(ModEntry.Instance.Status_SlowBurn.Status, mitigateAmount);
            return incomingDamage - mitigateAmount;
        }
        return incomingDamage;
    }

    public bool HandleStatusTurnAutoStep(IKokoroApi.IV2.IStatusLogicApi.IHook.IHandleStatusTurnAutoStepArgs args)
    {
        if (args.Status != ModEntry.Instance.Status_Mitigate.Status) return false;
        if (args.Timing != IKokoroApi.IV2.IStatusLogicApi.StatusTurnTriggerTiming.TurnStart) return false;
        if (args.Amount > 0)
        {
            args.Amount = 0;
        }
        return false;
    }

    public int ModifyStatusChange(IKokoroApi.IV2.IStatusLogicApi.IHook.IModifyStatusChangeArgs args)
    {
        if (args.Status == ModEntry.Instance.Status_Mitigate.Status && args.NewAmount > 0 && args.State.EnumerateAllArtifacts().Any(a => a is ForsakenSafety))
        {
            args.Ship.Add(ModEntry.Instance.KokoroApi.V2.DriveStatus.Pulsedrive, args.NewAmount);
            return 0;
        }
        return args.NewAmount;
    }

}