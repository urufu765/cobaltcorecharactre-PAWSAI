using System;
using System.Collections.Generic;
using Nickel;

namespace Starhunters.Bruno.Actions;

/// <summary>
/// Set the "damage" as you normally would, and also set the "baseDamage". It should automatically double the bonus damage.
/// </summary>
public class AHeavyAttack : AAttack
{
    public int baseDamage;

    public override void Begin(G g, State s, Combat c)
    {
        base.damage = Math.Max(damage, damage * 2 - baseDamage);
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
                tooltips[0] = new GlossaryTooltip("heavyAttackPierce")
                {
                    Title = ModEntry.Instance.Localizations.Localize(["Bruno", "action", "HeavyAttackPiercing", "title"]),
                    Icon = ModEntry.Instance.Action_HeavyAttack_Pierce,
                    Description = ModEntry.Instance.Localizations.Localize(["Bruno", "action", "HeavyAttackPiercing", "desc"]),
                    vals = [damage]
                };
            }
            else
            {
                tooltips[0] = new GlossaryTooltip("heavyAttack")
                {
                    Title = ModEntry.Instance.Localizations.Localize(["Bruno", "action", "HeavyAttack", "title"]),
                    Icon = ModEntry.Instance.Action_HeavyAttack,
                    Description = ModEntry.Instance.Localizations.Localize(["Bruno", "action", "HeavyAttack", "desc"]),
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
                piercing ? ModEntry.Instance.Action_HeavyAttack_Pierce : ModEntry.Instance.Action_HeavyAttack, damage, Colors.redd
            );
        }

        return new Icon
        (
            piercing ? ModEntry.Instance.Action_HeavyAttack_PierceFail : ModEntry.Instance.Action_HeavyAttack_Fail, damage, Colors.attackFail
        );
    }
}