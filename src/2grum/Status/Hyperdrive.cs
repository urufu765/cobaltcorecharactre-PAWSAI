using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Starhunters.Bruno.Artifacts;
using Starhunters.External;

namespace Starhunters.Bruno.Statuses;


public class Hyperdrive : IKokoroApi.IV2.IStatusLogicApi.IHook
{
    public Hyperdrive()
    {
        ModEntry.Instance.KokoroApi.V2.StatusLogic.RegisterHook(this);

        ModEntry.Instance.Harmony.Patch(
            original: AccessTools.DeclaredMethod(typeof(Card), nameof(Card.GetActionsOverridden)),
            postfix: new HarmonyMethod(MethodBase.GetCurrentMethod()!.DeclaringType!, nameof(CalculateHyperdriveDamage))
        );

        ModEntry.Instance.Harmony.Patch(
            original: AccessTools.DeclaredMethod(typeof(Combat), nameof(Combat.TryPlayCard)),
            postfix: new HarmonyMethod(MethodBase.GetCurrentMethod()!.DeclaringType!, nameof(KillHyperdrive))
        );
    }

    private static void KillHyperdrive(bool __result, Combat __instance, State s, Card card)
    {
        if (!__result) return;
        if (s.ship.Get(ModEntry.Instance.Status_Hyperdrive.Status) > 0)
        {
            List<CardAction> actions = card.GetActionsOverridden(s, __instance);

            foreach (CardAction action in actions)
            {
                if (ModEntry.Instance.Helper.ModData.TryGetModData(action, "killHyperdrive", out bool kill) && kill)
                {
                    s.ship.Set(ModEntry.Instance.Status_Hyperdrive.Status, 0);
                    break;
                }
            }
        }
    }


    // postfix getActionsOverridden, use reflection to find the first action with the "damage" field
    // Find the base damage manually by looping through a few baseDamage. If there's no change, then skip the below thing
    // Recalculate just that damage using GetDmg(s, baseDamage+Hyperdrive)
    // attach a moddata to that card to trigger Hyperdrive killing
    // check in Combat.TryPlayCard, then kill Hyperdrive upon use.
    // Profit.

    public static void CalculateHyperdriveDamage(Card __instance, ref List<CardAction> __result, State s)
    {
        for (int i = 0; i < __result.Count; i++)
        {
            if (__result[i].disabled)
            {
                continue;
            }
            // Finds an action with a field named "damage"
            FieldInfo? damageField = __result[i].GetType().GetField("damage", BindingFlags.Public | BindingFlags.Instance);
            if (damageField is not null && damageField.FieldType == typeof(int))
            {
                // Checks for targetPlayer if accurateCalculations is on. Just in case someone decided to add a self-damage attack to the card
                bool target = false;
                if (ModEntry.Instance.settings.ProfileBased.Current.AccurateCalculations)
                {
                    FieldInfo? targetField = __result[i].GetType().GetField("targetPlayer", BindingFlags.Public | BindingFlags.Instance);
                    if (targetField is not null && targetField.FieldType == typeof(bool))
                    {
                        target = (bool)targetField.GetValue(__result[i])!;
                    }
                }

                // Action's damage
                int actionDamage = (int)damageField.GetValue(__result[i])!;
                // Calculate baseDamage (brute-force)
                int baseDamage = FindBaseDamage(s, __instance, actionDamage, target);
                // Calculate new damage using baseDamage
                int newDamage = __instance.GetDmg(s, baseDamage + s.ship.Get(ModEntry.Instance.Status_Hyperdrive.Status), target);
                // Set this new value
                damageField.SetValue(__result[i], newDamage);


                // Set flag to this action so that when the card is used, it clears the hyperdrive.
                ModEntry.Instance.Helper.ModData.SetModData(__result[i], "killHyperdrive", true);
                break;
            }
        }
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
                return damageToMatch;
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