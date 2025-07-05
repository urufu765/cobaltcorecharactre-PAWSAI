using System.Collections.Generic;
using System.Reflection;
using Nanoray.PluginManager;
using Nickel;

namespace Starhunters.Kodijen.Cards;

/// <summary>
/// Kodijen.
/// </summary>
public class WeirdInvention : Card, IRegisterable
{
    public static void Register(IPluginPackage<IModManifest> package, IModHelper helper)
    {
        Rarity rare = Rarity.common;
        helper.Content.Cards.RegisterCard(new CardConfiguration
        {
            CardType = MethodBase.GetCurrentMethod()!.DeclaringType!,
            Meta = new CardMeta
            {
                rarity = rare,
                upgradesTo = [Upgrade.A, Upgrade.B],
                deck = ModEntry.Instance.KodijenDeck.Deck
            },
            Name = ModEntry.Instance.AnyLocalizations.Bind(["Kodijen", "card", rare.ToString(), MethodBase.GetCurrentMethod()!.DeclaringType!.Name, "name"]).Localize,
            Art = ModEntry.RegisterSprite(package, $"assets/card/kodijen/{rare}/{MethodBase.GetCurrentMethod()!.DeclaringType!.Name}.png").Sprite
        });
    }


    public override List<CardAction> GetActions(State s, Combat c)
    {
        return upgrade switch
        {
            Upgrade.B =>
            [
                new ASpawn
                {
                    thing = new DualDrone()
                },
                new AStatus
                {
                    status = Status.tempShield,
                    statusAmount = 2,
                    targetPlayer = true
                },
                new AStatus
                {
                    status = Status.droneShift,
                    statusAmount = 1,
                    targetPlayer = true
                }
            ],
            _ =>
            [
                new ASpawn
                {
                    thing = new DualDrone()
                },
                new AStatus
                {
                    status = Status.tempShield,
                    statusAmount = 2,
                    targetPlayer = true
                }
            ],
        };
    }


    public override CardData GetData(State state)
    {
        return upgrade switch
        {
            Upgrade.A => new CardData
            {
                cost = 0,
            },
            _ => new CardData
            {
                cost = 1,
            }
        };
    }
}