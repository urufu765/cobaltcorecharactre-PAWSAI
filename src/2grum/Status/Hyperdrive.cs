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

public class Hyperdrive : IKokoroApi.IV2.IStatusLogicApi.IHook
{
    public Hyperdrive()
    {
        ModEntry.Instance.KokoroApi.V2.StatusLogic.RegisterHook(this);

        // To mark all the actions that will be compensated by Hyperdrive
        ModEntry.Instance.Harmony.Patch(
            original: AccessTools.DeclaredMethod(typeof(Card), nameof(Card.GetActionsOverridden)),
            postfix: new HarmonyMethod(MethodBase.GetCurrentMethod()!.DeclaringType!, nameof(CalculateHyperdriveDamage))
        );

        // To show visually what Hyperdrive is doing
        ModEntry.Instance.Harmony.Patch(
            original: AccessTools.DeclaredMethod(typeof(Card), nameof(Card.MakeAllActionIcons)),
            transpiler: new HarmonyMethod(MethodBase.GetCurrentMethod()!.DeclaringType!, nameof(ShowHyperdriveDamage))
        );

        // And finally to actually add the damage and kill Hyperdrive
        ModEntry.Instance.Harmony.Patch(
            original: AccessTools.DeclaredMethod(typeof(Combat), nameof(Combat.BeginCardAction)),
            prefix: new HarmonyMethod(MethodBase.GetCurrentMethod()!.DeclaringType!, nameof(ActuallyDoHyperdriveDamage))
        );

        // ModEntry.Instance.Harmony.Patch(
        //     original: AccessTools.DeclaredMethod(typeof(Combat), nameof(Combat.TryPlayCard)),
        //     postfix: new HarmonyMethod(MethodBase.GetCurrentMethod()!.DeclaringType!, nameof(KillHyperdrive))
        // );
    }

    private static void ActuallyDoHyperdriveDamage(G g, ref CardAction a)
    {
        if (g.state.ship.Get(ModEntry.Instance.Status_Hyperdrive.Status) > 0)
        {
            ModEntry.Instance.Logger.LogInformation("Hyperdrivetime");

            if (ModEntry.Instance.Helper.ModData.TryGetModData(a, "doHyperdrive", out int newDamage))
            {
                ModEntry.Instance.Logger.LogInformation("Wahb" + newDamage);
                FieldInfo? damageField = a.GetType().GetField("damage", BindingFlags.Public | BindingFlags.Instance);
                if (damageField is not null && damageField.FieldType == typeof(int))
                {
                    damageField.SetValue(a, newDamage);
                }
                g.state.ship.Set(ModEntry.Instance.Status_Hyperdrive.Status, 0);
            }
        }
    }

    private static IEnumerable<CodeInstruction> ShowHyperdriveDamage(IEnumerable<CodeInstruction> instructions, ILGenerator il)
    {
        try
        {
            return new SequenceBlockMatcher<CodeInstruction>(instructions)
                .Find(
                    SequenceBlockMatcherFindOccurence.First,
                    SequenceMatcherRelativeBounds.WholeSequence,
                    ILMatches.AnyCall,
                    ILMatches.AnyStloc.GetLocalIndex(out var index)
                )
                .Insert(
                    SequenceMatcherPastBoundsDirection.After,
                    SequenceMatcherInsertionResultingBounds.JustInsertion,
                    [
                        new(OpCodes.Ldarg_0),
                        new(OpCodes.Ldarg_2),
                        new(OpCodes.Ldloc, index.Value),
                        new(OpCodes.Call, AccessTools.DeclaredMethod(typeof(Hyperdrive), nameof(ShowCalculatedHyperdriveDamage))),
                        new(OpCodes.Stloc, index.Value),
                    ]
                ).AllElements();
        }
        catch (Exception err)
        {
            ModEntry.Instance.Logger.LogError(err, "Blastit!!!");
            throw;
        }
    }

    private static List<CardAction> ShowCalculatedHyperdriveDamage(Card card, State s, List<CardAction> cardActions)
    {
        if (s.ship.Get(ModEntry.Instance.Status_Hyperdrive.Status) > 0)
        {
            List<CardAction> copyActions = cardActions;
            for (int i = 0; i < cardActions.Count; i++)
            {
                if (copyActions[i].disabled) continue;

                FieldInfo? actionField = copyActions[i].GetType().GetField("action", BindingFlags.Public | BindingFlags.Instance);
                PropertyInfo? actionProperty = copyActions[i].GetType().GetProperty("Action", BindingFlags.Public | BindingFlags.Instance);
                if (
                    (
                        actionField is not null &&
                        actionField.FieldType == typeof(CardAction) &&
                        SetDamage(card, (CardAction)actionField.GetValue(copyActions[i])!, s, setDamageOnly: true)
                    ) ||
                    (
                        actionProperty is not null &&
                        actionProperty.PropertyType == typeof(CardAction) &&
                        SetDamage(card, (CardAction)actionProperty.GetValue(copyActions[i])!, s, setDamageOnly: true)
                    )
                )
                {
                    continue;
                }

                if (SetDamage(card, copyActions[i], s, setDamageOnly: true))
                {
                    break;
                }
            }
            return copyActions;
        }
        return cardActions;
    }

    // private static void KillHyperdrive(bool __result, Combat __instance, State s, Card card)
    // {
    //     if (!__result) return;
    //     if (s.ship.Get(ModEntry.Instance.Status_Hyperdrive.Status) > 0)
    //     {
    //         List<CardAction> actions = card.GetActionsOverridden(s, __instance);

    //         foreach (CardAction action in actions)
    //         {
    //             if (ModEntry.Instance.Helper.ModData.TryGetModData(action, "killHyperdrive", out bool kill) && kill)
    //             {
    //                 s.ship.Set(ModEntry.Instance.Status_Hyperdrive.Status, 0);
    //                 break;
    //             }
    //         }
    //     }
    // }


    // postfix getActionsOverridden, use reflection to find the first action with the "damage" field
    // Find the base damage manually by looping through a few baseDamage. If there's no change, then skip the below thing
    // Recalculate just that damage using GetDmg(s, baseDamage+Hyperdrive)
    // attach a moddata to that card to trigger Hyperdrive killing
    // check in Combat.TryPlayCard, then kill Hyperdrive upon use.
    // Profit.

    public static void CalculateHyperdriveDamage(Card __instance, ref List<CardAction> __result, State s)
    {
        if (s.ship.Get(ModEntry.Instance.Status_Hyperdrive.Status) <= 0) return;
        for (int i = 0; i < __result.Count; i++)
        {
            if (__result[i].disabled)
            {
                continue;
            }

            FieldInfo? actionField = __result[i].GetType().GetField("action", BindingFlags.Public | BindingFlags.Instance);
            PropertyInfo? actionProperty = __result[i].GetType().GetProperty("Action", BindingFlags.Public | BindingFlags.Instance);
            if (
                (
                    actionField is not null &&
                    actionField.FieldType == typeof(CardAction) &&
                    SetDamage(__instance, (CardAction)actionField.GetValue(__result[i])!, s, true)
                ) ||
                (
                    actionProperty is not null &&
                    actionProperty.PropertyType == typeof(CardAction) &&
                    SetDamage(__instance, (CardAction)actionProperty.GetValue(__result[i])!, s, true)
                )
            )
            {
                continue;
            }

            if (SetDamage(__instance, __result[i], s, true))
            {
                break;
            }
        }
    }

    private static bool SetDamage(Card card, CardAction action, State s, bool setModDataOnly = false, bool setDamageOnly = false)
    {
        // Finds an action with a field named "damage"
        FieldInfo? damageField = action.GetType().GetField("damage", BindingFlags.Public | BindingFlags.Instance);
        if (damageField is not null && damageField.FieldType == typeof(int))
        {
            // Checks for targetPlayer if accurateCalculations is on. Just in case someone decided to add a self-damage attack to the card
            bool target = false;
            if (ModEntry.Instance.settings.ProfileBased.Current.AccurateCalculations)
            {
                FieldInfo? targetField = action.GetType().GetField("targetPlayer", BindingFlags.Public | BindingFlags.Instance);
                if (targetField is not null && targetField.FieldType == typeof(bool))
                {
                    target = (bool)targetField.GetValue(action)!;
                }
            }

            // Action's damage
            int actionDamage = (int)damageField.GetValue(action)!;
            // Calculate baseDamage (brute-force)
            int baseDamage = FindBaseDamage(s, card, actionDamage, target);
            // Calculate new damage using baseDamage
            int newDamage = card.GetDmg(s, baseDamage + s.ship.Get(ModEntry.Instance.Status_Hyperdrive.Status), target);

            if (!setModDataOnly)
            {
                // Set this new value
                damageField.SetValue(action, newDamage);
            }

            if (!setDamageOnly)
            {
                // Set flag to this action so that when the card is used, it clears the hyperdrive.
                ModEntry.Instance.Helper.ModData.SetModData(action, "doHyperdrive", newDamage);
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// Brute-force calculates base damage
    /// </summary>
    /// <param name="s"></param>
    /// <param name="card"></param>
    /// <param name="damageToMatch"></param>
    /// <param name="targetPlayer"></param>
    /// <returns></returns>
    private static int FindBaseDamage(State s, Card card, int damageToMatch, bool targetPlayer = false)
    {
        int result = 0;
        int maxTries = 10;
        int lastVal = 0;
        int lastResult = 0;
        int repeatedVal = 0;
        int maxRepetition = 3;
        int bestTry = 0;
        int offBy = 999;

        for (; result < maxTries && repeatedVal < maxRepetition; result++)
        {
            int calculate = card.GetDmg(s, result, targetPlayer);
            if (calculate == damageToMatch)  // Exact match
            {
                return result;
            }
            else if (calculate > damageToMatch)  // Guess has passed match
            {
                if (calculate - damageToMatch < offBy)
                {
                    return result;
                }
                else
                {
                    return bestTry;
                }
            }
            else  // Keep updating best guess
            {
                bestTry = result;
                offBy = damageToMatch - calculate;
            }

            if (lastVal == calculate)  // For repeating results
            {
                repeatedVal++;
            }
            else
            {
                repeatedVal = 0;
                lastVal = calculate;
                lastResult = result;
            }
        }

        return lastResult;
    }
}