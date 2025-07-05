using System.Collections.Generic;
using System.Reflection;
using Nanoray.PluginManager;
using Nickel;
using Starhunters.Kodijen.Objects;

namespace Starhunters.Kodijen.Cards;

/// <summary>
/// Kodijen.
/// </summary>
public class SuperMissileDrone : Card, IRegisterable, IHasCustomCardTraits
{
    public static void Register(IPluginPackage<IModManifest> package, IModHelper helper)
    {
        Rarity rare = Rarity.uncommon;
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
        ModEntry.Instance.KokoroApi.V2.Limited.SetBaseLimitedUses(ice.UniqueName, Upgrade.B, 2);
    }


    public override List<CardAction> GetActions(State s, Combat c)
    {
        return upgrade switch
        {
            Upgrade.B =>
            [
                new ASpawn
                {
                    thing = new MissileDrone{upgraded = true, bubbleShield = true}
                }
            ],
            _ =>
            [
                new ASpawn
                {
                    thing = new MissileDrone{upgraded = true}
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
                cost = 2,
            }
        };
    }

    public IReadOnlySet<ICardTraitEntry> GetInnateTraits(State state)
    {
        return new HashSet<ICardTraitEntry> { ModEntry.Instance.KokoroApi.V2.Limited.Trait };
    }

}