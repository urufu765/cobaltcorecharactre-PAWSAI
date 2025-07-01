using System.Collections.Generic;
using System.Reflection;
using Nanoray.PluginManager;
using Nickel;
using Starhunters.Bruno.Actions;

namespace Starhunters.Bruno.Cards;

/// <summary>
/// Bruno.
/// </summary>
public class OptimizedBlast : Card, IRegisterable
{
    public static void Register(IPluginPackage<IModManifest> package, IModHelper helper)
    {
        Rarity rare = Rarity.uncommon;
        helper.Content.Cards.RegisterCard(new CardConfiguration
        {
            CardType = MethodBase.GetCurrentMethod()!.DeclaringType!,
            Meta = new CardMeta
            {
                rarity = rare,
                upgradesTo = [Upgrade.A, Upgrade.B],
                deck = ModEntry.Instance.BrunoDeck.Deck
            },
            Name = ModEntry.Instance.AnyLocalizations.Bind(["Bruno", "card", rare.ToString(), MethodBase.GetCurrentMethod()!.DeclaringType!.Name, "name"]).Localize,
            Art = ModEntry.RegisterSprite(package, $"assets/card/bruno/{rare}/{MethodBase.GetCurrentMethod()!.DeclaringType!.Name}.png").Sprite
        });
    }


    public override List<CardAction> GetActions(State s, Combat c)
    {
        return upgrade switch
        {
            Upgrade.B =>
            [
                new AHeavyAttack
                {
                    damage = GetDmg(s, 1),
                    baseDamage = 1
                },
                new AStatus
                {
                    status = ModEntry.Instance.Status_SlowBurn.Status,
                    statusAmount = -2,
                    targetPlayer = true,
                },
                new AStatus
                {
                    status = ModEntry.Instance.Status_Mitigate.Status,
                    statusAmount = 2,
                    targetPlayer = true
                }
            ],
            Upgrade.A =>
            [
                new AHeavyAttack
                {
                    damage = GetDmg(s, 1),
                    baseDamage = 1
                },
                new AStatus
                {
                    status = ModEntry.Instance.Status_SlowBurn.Status,
                    statusAmount = -4,
                    targetPlayer = true,
                }
            ],
            _ =>
            [
                new AHeavyAttack
                {
                    damage = GetDmg(s, 1),
                    baseDamage = 1
                },
                new AStatus
                {
                    status = ModEntry.Instance.Status_SlowBurn.Status,
                    statusAmount = -2,
                    targetPlayer = true,
                }
            ],
        };
    }


    public override CardData GetData(State state)
    {
        return upgrade switch
        {
            _ => new CardData
            {
                cost = 1,
            }
        };
    }
}