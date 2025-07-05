using System.Collections.Generic;
using System.Reflection;
using Nanoray.PluginManager;
using Nickel;

namespace Starhunters.Kodijen.Cards;

/// <summary>
/// Kodijen.
/// </summary>
public class RepairSchedule : Card, IRegisterable, IHasCustomCardTraits
{
    private static Rarity rare = Rarity.uncommon;
    public static void Register(IPluginPackage<IModManifest> package, IModHelper helper)
    {
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

        ModEntry.Instance.KokoroApi.V2.Limited.SetBaseLimitedUses(ice.UniqueName, Upgrade.None, 2);
        ModEntry.Instance.KokoroApi.V2.Limited.SetBaseLimitedUses(ice.UniqueName, Upgrade.A, 3);
    }


    public override List<CardAction> GetActions(State s, Combat c)
    {
        int x = s.ship.hullMax - s.ship.hull;
        int req = upgrade switch
        {
            Upgrade.B => 2,
            Upgrade.A => 4,
            _ => 5
        };
        if (x > req)
        {
            return
            [
                new ASpawn
                {
                    thing = new RepairKit{yAnimation = 0}
                }
            ];
        }
        return [];
    }


    public override CardData GetData(State state)
    {
        int x = state.ship.hullMax - state.ship.hull;
        CardData cd = upgrade switch
        {
            Upgrade.B => new CardData
            {
                cost = 1,
                exhaust = true
            },
            _ => new CardData
            {
                cost = 1
            }
        };

        cd.description = ModEntry.Instance.Localizations.Localize(["Kodijen", "card", rare.ToString(), MethodBase.GetCurrentMethod()!.DeclaringType!.Name, "desc"
            ], new
            {
                requirement = upgrade switch
                {
                    Upgrade.B => 2,
                    Upgrade.A => 4,
                    _ => 5
                },
                amount = x
            });
        return cd;
    }

    public IReadOnlySet<ICardTraitEntry> GetInnateTraits(State state)
    {
        return upgrade == Upgrade.B ? new HashSet<ICardTraitEntry> { ModEntry.Instance.KokoroApi.V2.Fleeting.Trait } : new HashSet<ICardTraitEntry> { ModEntry.Instance.KokoroApi.V2.Fleeting.Trait, ModEntry.Instance.KokoroApi.V2.Limited.Trait };
    }

}