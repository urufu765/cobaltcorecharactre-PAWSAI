using System.Collections.Generic;
using System.Reflection;
using Nanoray.PluginManager;
using Nickel;

namespace Starhunters.Kodijen.Cards;

/// <summary>
/// Kodijen.
/// </summary>
public class PreparationH : Card, IRegisterable, IHasCustomCardTraits
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
                new AStatus
                {
                    status = ModEntry.Instance.Status_HullCapacity.Status,
                    statusAmount = 2,
                    targetPlayer = true
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
                },
            ],
            _ =>
            [
                new AStatus
                {
                    status = ModEntry.Instance.Status_HullCapacity.Status,
                    statusAmount = 2,
                    targetPlayer = true
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
                    statusAmount = 2,
                    targetPlayer = true
                },
            ],
        };
    }


    public override CardData GetData(State state)
    {
        return upgrade switch
        {
            Upgrade.B => new CardData
            {
                cost = 1,
                buoyant = true,
                infinite = true
            },
            Upgrade.A => new CardData
            {
                cost = 0,
                buoyant = true,
                exhaust = true
            },
            _ => new CardData
            {
                cost = 1,
                buoyant = true,
                exhaust = true
            }
        };
    }

    public IReadOnlySet<ICardTraitEntry> GetInnateTraits(State state)
    {
        return upgrade == Upgrade.B ? new HashSet<ICardTraitEntry> { ModEntry.Instance.KokoroApi.V2.Fleeting.Trait } : [];
    }

}