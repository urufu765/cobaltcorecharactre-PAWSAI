using System;

namespace Starhunters.Bruno.Actions;

public static class BrunoAttackHelper
{
    public static void DoExtraAttackStuff(ref AAttack __instance, State s, Combat c)
    {
        Ship toShip = __instance.targetPlayer ? s.ship : c.otherShip;
        Ship fromShip = __instance.targetPlayer ? c.otherShip : s.ship;

        if (__instance is ABreachAttack aba)
        {
            if (toShip.Get(Status.shield) + toShip.Get(Status.tempShield) > 0)
            {
                aba.doubleDamage = true;
            }

            if (fromShip.Get(ModEntry.Instance.Status_Hamper.Status) > 0)
            {
                aba.overrideDamage = true;
            }
        }

        if (__instance is AHeavyAttack aha && fromShip.Get(ModEntry.Instance.Status_Hamper.Status) > 0)
        {
            aha.overrideDamage = true;
        }
        
    }

    public static void SlapSomeRecoil(AAttack __instance, Combat c)
    {
        if (__instance is ABreachAttack or AHeavyAttack)
        {
            c.QueueImmediate(new AStatus
            {
                status = ModEntry.Instance.Status_Recoil.Status,
                statusAmount = 1,
                targetPlayer = !__instance.targetPlayer
            });
        }
    }
}