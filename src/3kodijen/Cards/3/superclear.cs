using System.Collections.Generic;
using System.Reflection;
using Nanoray.PluginManager;
using Nickel;
using Starhunters.Kodijen.Actions;

namespace Starhunters.Kodijen.Cards;

/// <summary>
/// Kodijen.
/// </summary>
public class SuperClear : Card, IRegisterable, IHasCustomCardTraits
{
    private static Rarity rare = Rarity.rare;
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
        ModEntry.Instance.KokoroApi.V2.Limited.SetBaseLimitedUses(ice.UniqueName, Upgrade.A, 4);
    }


    public override List<CardAction> GetActions(State s, Combat c)
    {
        return
        [
            new ADestroyOutsideDrones
            {
                checkEnemy = flipped
            }
        ];
    }


    public override CardData GetData(State state)
    {
        CardData cd = upgrade switch
        {
            Upgrade.B => new CardData
            {
                cost = 0,
                flippable = true,
                exhaust = true
            },
            _ => new CardData
            {
                cost = 2,
                flippable = true
            }
        };

        cd.description = ModEntry.Instance.Localizations.Localize(["Kodijen", "card", rare.ToString(), MethodBase.GetCurrentMethod()!.DeclaringType!.Name, "desc"
            ], new
            {
                ship = ModEntry.Instance.Localizations.Localize(["Kodijen", "card", rare.ToString(), MethodBase.GetCurrentMethod()!.DeclaringType!.Name, flipped? "enemy" : "player"])
            });
        return cd;
    }

    public IReadOnlySet<ICardTraitEntry> GetInnateTraits(State state)
    {
        return upgrade == Upgrade.B ? [] : new HashSet<ICardTraitEntry> { ModEntry.Instance.KokoroApi.V2.Limited.Trait };
    }

}