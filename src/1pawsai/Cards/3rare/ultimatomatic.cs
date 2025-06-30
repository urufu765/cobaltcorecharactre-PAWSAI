using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Nanoray.PluginManager;
using Nickel;

namespace Starhunters.Pawsai.Cards;

/// <summary>
/// Pawsai.
/// </summary>
public class Ultimatomatic : Card, IRegisterable
{
    private static Rarity rare = Rarity.rare;
    public static void Register(IPluginPackage<IModManifest> package, IModHelper helper)
    {
        helper.Content.Cards.RegisterCard(new CardConfiguration
        {
            CardType = MethodBase.GetCurrentMethod()!.DeclaringType!,
            Meta = new CardMeta
            {
                rarity = rare,
                upgradesTo = [Upgrade.A, Upgrade.B]
            },
            Name = ModEntry.Instance.AnyLocalizations.Bind(["Pawsai", "card", rare.ToString(), MethodBase.GetCurrentMethod()!.DeclaringType!.Name, "name"]).Localize,
            Art = ModEntry.RegisterSprite(package, $"assets/card/pawsai/{rare}/{MethodBase.GetCurrentMethod()!.DeclaringType!.Name}.png").Sprite
        });
    }


    public override List<CardAction> GetActions(State s, Combat c)
    {
        int x = s.ship.GetMaxShield() - s.ship.Get(Status.shield);
        return upgrade switch
        {
            Upgrade.B =>
            [
                .. Enumerable.Range(0, x * 2)
                    .Select(_ => new AAttack
                    {
                        damage = GetDmg(s, 1),
                        fast = true,
                        piercing = true
                    })
                    .Cast<CardAction>(),
                new AStatus
                {
                    status = Status.maxShield,
                    statusAmount = -x,
                    targetPlayer = true
                }
            ],
            Upgrade.A =>
            [
                .. Enumerable.Range(0, x * 3)
                    .Select(_ => new AAttack
                    {
                        damage = GetDmg(s, 1),
                        fast = true
                    })
                    .Cast<CardAction>(),
                new AStatus
                {
                    status = Status.maxShield,
                    statusAmount = -x,
                    targetPlayer = true
                }
            ],
            _ =>
            [
                .. Enumerable.Range(0, x * 2)
                    .Select(_ => new AAttack
                    {
                        damage = GetDmg(s, 1),
                        fast = true
                    })
                    .Cast<CardAction>(),
                new AStatus
                {
                    status = Status.maxShield,
                    statusAmount = -x,
                    targetPlayer = true
                }
            ],
        };
    }


    public override CardData GetData(State state)
    {
        CardData cd = upgrade switch
        {
            _ => new CardData
            {
                cost = 3,
                exhaust = true
            }
        };

        cd.description = ModEntry.Instance.Localizations.Localize(["Pawsai", "card", rare.ToString(), MethodBase.GetCurrentMethod()!.DeclaringType!.Name, upgrade switch {
                Upgrade.B => "descB",
                Upgrade.A => "descA",
                _ => "desc"
            }], new List<string>()
        {
            ModEntry.Instance.Localizations.Localize(["Pawsai", "card", rare.ToString(), MethodBase.GetCurrentMethod()!.DeclaringType!.Name, state.ship.Get(Status.stunCharge) > 0? "s" : "f"]),
        });
        return cd;
    }
}