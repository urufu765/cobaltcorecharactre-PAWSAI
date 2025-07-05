using System.Collections.Generic;
using System.Reflection;
using Nanoray.PluginManager;
using Nickel;

namespace Starhunters.Kodijen.Cards;

/// <summary>
/// Kodijen.
/// </summary>
public class WallOGun : Card, IRegisterable, IHasCustomCardTraits
{
    public static void Register(IPluginPackage<IModManifest> package, IModHelper helper)
    {
        Rarity rare = Rarity.rare;
        ICardEntry ice = helper.Content.Cards.RegisterCard(new CardConfiguration
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

        ModEntry.Instance.KokoroApi.V2.Limited.SetBaseLimitedUses(ice.UniqueName, Upgrade.B, 3);
    }


    public override List<CardAction> GetActions(State s, Combat c)
    {
        return upgrade switch
        {
            _ =>
            [
                new ASpawn
                {
                    thing = new AttackDrone(),
                    offset = -2
                },
                new ASpawn
                {
                    thing = new AttackDrone(),
                    offset = -1
                },
                new ASpawn
                {
                    thing = new AttackDrone(),
                    offset = 0
                },
                new ASpawn
                {
                    thing = new AttackDrone(),
                    offset = 1
                },
                new ASpawn
                {
                    thing = new AttackDrone(),
                    offset = 2
                }
            ],
        };
    }


    public override CardData GetData(State state)
    {
        return upgrade switch
        {
            Upgrade.B => new CardData
            {
                cost = 4,
            },
            Upgrade.A => new CardData
            {
                cost = 3,
                exhaust = true
            },
            _ => new CardData
            {
                cost = 4,
                exhaust = true
            }
        };
    }

    public IReadOnlySet<ICardTraitEntry> GetInnateTraits(State state)
    {
        return upgrade == Upgrade.B ? new HashSet<ICardTraitEntry> { ModEntry.Instance.KokoroApi.V2.Limited.Trait, ModEntry.Instance.KokoroApi.V2.Fleeting.Trait } : [];
    }

}