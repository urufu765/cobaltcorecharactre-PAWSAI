using System.Collections.Generic;
using Nickel;

namespace Starhunters.Bruno.Actions;

public class ABreachAttack : AAttack
{
    public override void Begin(G g, State s, Combat c)
    {
        if (WillHitShieldedFoe(s))
        {
            damage *= 2;
        }
        base.Begin(g, s, c);
        c.QueueImmediate(new AStatus
        {
            status = ModEntry.Instance.Status_Recoil.Status,
            statusAmount = 1,
            targetPlayer = !targetPlayer
        });
    }

    private bool WillHitShieldedFoe(State s)
    {
        if (s.route is Combat c)
        {
            Ship toShip = targetPlayer ? s.ship : c.otherShip;
            Ship fromShip = targetPlayer ? c.otherShip : s.ship;
            bool hitShield = toShip.Get(Status.shield) + toShip.Get(Status.tempShield) > 0;

            // If it hits a drone, do double damage to drone if bubble shield.
            int? n = GetFromX(s, c);
            RaycastResult? raycast = null;
            if (fromDroneX is int dx)
            {
                raycast = CombatUtils.RaycastGlobal(c, toShip, true, dx);
            }
            else if (n is int nx)
            {
                raycast = CombatUtils.RaycastFromShipLocal(s, c, nx, targetPlayer);
            }
            if (raycast is RaycastResult raycastResult && raycastResult.hitDrone)
            {
                hitShield = c.stuff[raycastResult.worldX].bubbleShield;
            }

            // Since Hamper would like this attack to only do 1 damage.
            if (fromShip.Get(ModEntry.Instance.Status_Hamper.Status) > 0)
            {
                hitShield = false;
            }

            return hitShield;
        }
        return false;
    }

    public override List<Tooltip> GetTooltips(State s)
    {
        List<Tooltip> tooltips = base.GetTooltips(s);

        // Assumes the first item is the attack text
        if (tooltips.Count > 0)
        {
            if (piercing)
            {
                tooltips[0] = new GlossaryTooltip("breachAttackPierce")
                {
                    Title = ModEntry.Instance.Localizations.Localize(["Bruno", "action", "BreachAttackPiercing", "title"]),
                    Icon = ModEntry.Instance.Action_BreachAttack_Pierce,
                    Description = ModEntry.Instance.Localizations.Localize(["Bruno", "action", "BreachAttackPiercing", "desc"]),
                    vals = [damage]
                };
            }
            else
            {
                tooltips[0] = new GlossaryTooltip("breachAttack")
                {
                    Title = ModEntry.Instance.Localizations.Localize(["Bruno", "action", "BreachAttack", "title"]),
                    Icon = ModEntry.Instance.Action_BreachAttack,
                    Description = ModEntry.Instance.Localizations.Localize(["Bruno", "action", "BreachAttack", "desc"]),
                    vals = [damage]
                };
            }
        }
        tooltips.AddRange(StatusMeta.GetTooltips(ModEntry.Instance.Status_Recoil.Status, 1));
        return tooltips;
    }


    public override Icon? GetIcon(State s)
    {
        if (DoWeHaveCannonsThough(s))
        {
            return new Icon
            (
                piercing ? ModEntry.Instance.Action_BreachAttack_Pierce : ModEntry.Instance.Action_BreachAttack, damage, WillHitShieldedFoe(s)? Colors.cheevoGold : Colors.redd
            );
        }

        return new Icon
        (
            piercing ? ModEntry.Instance.Action_BreachAttack_PierceFail : ModEntry.Instance.Action_BreachAttack_Fail, damage, Colors.attackFail
        );
    }
}