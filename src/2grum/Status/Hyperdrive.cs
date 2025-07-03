using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        // To show on descriptions
        ModEntry.Instance.Harmony.Patch(
            original: AccessTools.DeclaredMethod(typeof(Card), nameof(Card.GetDmg)),
            prefix: new HarmonyMethod(MethodBase.GetCurrentMethod()!.DeclaringType!, nameof(AttemptToShowHyperdriveDamageOnCardText))
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

    private static void AttemptToShowHyperdriveDamageOnCardText(State s, ref int baseDamage)
    {
        if (ModEntry.Instance.settings.ProfileBased.Current.Bruno_FancyHyperdrive && s.ship.Get(ModEntry.Instance.Status_Hyperdrive.Status) > 0)
        {
            var stackTrace = new StackTrace();
            if (
                stackTrace.GetFrame(1)?.GetMethod()?.Name is string try1 && try1 == "GetData" ||
                stackTrace.GetFrame(2)?.GetMethod()?.Name is string try2 && try2 == "GetData"
            )
            {
                baseDamage += s.ship.Get(ModEntry.Instance.Status_Hyperdrive.Status);
            }
        }
    }

    /**
    - [ISSUE!] From multiple prefixes of action getters, set Count of damage actions the card contains, as well as the number of wrapped actions (OnDraw, GetData, GetActions, OnDiscard, OnOtherCardPlayedWhileThisWasInHand, OnFlip), as well as set moddata for the card
    - Every call to GetDmg, count. Stop at index MIN(1 + wrapped, Count)
    - Until stop, apply Hyperdrive
    - In card use, kill hyperdrive if moddata is present
    */

    private static void ActuallyDoHyperdriveDamage(Combat __instance, G g, ref CardAction a)
    {
        if (g.state.ship.Get(ModEntry.Instance.Status_Hyperdrive.Status) > 0)
        {
            //ModEntry.Instance.Logger.LogInformation("Hyperdrivetime");
            FieldInfo? damageField = a.GetType().GetField("damage", BindingFlags.Public | BindingFlags.Instance);
            if (damageField is not null && damageField.FieldType == typeof(int))
            {
                if (ModEntry.Instance.Helper.ModData.TryGetModData(a, "doHyperdrive", out int newDamage))
                {
                    g.state.ship.Set(ModEntry.Instance.Status_Hyperdrive.Status, 0);
                    damageField.SetValue(a, newDamage);
                }
                else  // Just a catch all for attacks that are like from weird stuff like Card.OnDraw()
                {
                    FieldInfo? droneField = a.GetType().GetField("fromDroneX", BindingFlags.Public | BindingFlags.Instance);
                    if (droneField is not null && droneField.FieldType == typeof(int?) && droneField.GetValue(a) is int)
                    {
                        return;  // Don't let drones steal the hyperdrive
                    }
                    bool target = false;
                    FieldInfo? targetField = a.GetType().GetField("targetPlayer", BindingFlags.Public | BindingFlags.Instance);
                    if (targetField is not null && targetField.FieldType == typeof(bool))
                    {
                        target = (bool)targetField.GetValue(a)!;
                    }
                    Card? card = TryGetWhateverCard(__instance);
                    int actionDamage = (int)damageField.GetValue(a)!;
                    int baseDamage = FindBaseDamage(g.state, card, actionDamage, target);
                    int guessedDamage = card is not null ? card.GetDmg(g.state, baseDamage, target) : Card.GetActualDamage(g.state, baseDamage, target);
                    damageField.SetValue(a, guessedDamage);
                }
            }
        }
    }

    private static Card? TryGetWhateverCard(Combat c)
    {
        if (c.hand.Count > 0)
        {
            return c.hand[0];
        }
        if (c.discard.Count > 0)
        {
            return c.discard[0];
        }
        if (c.exhausted.Count > 0)
        {
            return c.exhausted[0];
        }
        return null;
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

                // Skip Pawsai's OffensiveDefense thing
                if (ModEntry.Instance.Helper.ModData.TryGetModData(copyActions[i], "noTouch", out bool noNo) && noNo)
                {
                    continue;
                }

                if (ModEntry.Instance.KokoroApi.V2.WrappedActions.GetWrappedCardActions(copyActions[i]) is not null)
                {
                    foreach (CardAction cardAction in ModEntry.Instance.KokoroApi.V2.WrappedActions.GetWrappedCardActionsRecursively(copyActions[i]))
                    {
                        SetDamage(card, cardAction, s, true);
                    }
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

            // Skip Pawsai's OffensiveDefense thing
            if (ModEntry.Instance.Helper.ModData.TryGetModData(__result[i], "noTouch", out bool noNo) && noNo)
            {
                continue;
            }

            if (ModEntry.Instance.KokoroApi.V2.WrappedActions.GetWrappedCardActions(__result[i]) is not null)
            {
                foreach (CardAction cardAction in ModEntry.Instance.KokoroApi.V2.WrappedActions.GetWrappedCardActionsRecursively(__result[i]))
                {
                    SetDamage(__instance, cardAction, s, true);
                }
                continue;
            }

            // FieldInfo? actionField = __result[i].GetType().GetField("action", BindingFlags.Public | BindingFlags.Instance);
            // PropertyInfo? actionProperty = __result[i].GetType().GetProperty("Action", BindingFlags.Public | BindingFlags.Instance);
            // if (
            //     (
            //         actionField is not null &&
            //         actionField.FieldType == typeof(CardAction) &&
            //         SetDamage(__instance, (CardAction)actionField.GetValue(__result[i])!, s, true)
            //     ) ||
            //     (
            //         actionProperty is not null &&
            //         actionProperty.PropertyType == typeof(CardAction) &&
            //         SetDamage(__instance, (CardAction)actionProperty.GetValue(__result[i])!, s, true)
            //     )
            // )
            // {
            //     continue;
            // }

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
            if (ModEntry.Instance.settings.ProfileBased.Current.Bruno_FancyHyperdrive)
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
    private static int FindBaseDamage(State s, Card? card, int damageToMatch, bool targetPlayer = false)
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
            int calculate;
            if (card is Card c)
            {
                calculate = c.GetDmg(s, result, targetPlayer);
            }
            else
            {
                calculate = Card.GetActualDamage(s, result, targetPlayer);
            }

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