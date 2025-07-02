using System.Collections.Generic;
using Nickel;

namespace Starhunters.Bruno.Actions;

public class ABreachAttack : AAttack
{
    public override void Begin(G g, State s, Combat c)
    {
        Ship fromShip = targetPlayer ? c.otherShip : s.ship;
        Ship toShip = targetPlayer ? s.ship : c.otherShip;

        if (toShip.Get(Status.shield) + toShip.Get(Status.tempShield) > 0 && fromShip.Get(ModEntry.Instance.Status_Hamper.Status) <= 0)
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
                piercing ? ModEntry.Instance.Action_BreachAttack_Pierce : ModEntry.Instance.Action_BreachAttack, damage, Colors.redd
            );
        }

        return new Icon
        (
            piercing ? ModEntry.Instance.Action_BreachAttack_PierceFail : ModEntry.Instance.Action_BreachAttack_Fail, damage, Colors.attackFail
        );
    }
}