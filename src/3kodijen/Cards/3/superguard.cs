using System.Collections.Generic;
using System.Reflection;
using Nanoray.PluginManager;
using Nickel;

namespace Starhunters.Kodijen.Cards;

/// <summary>
/// Kodijen.
/// </summary>
public class SuperGuard : Card, IRegisterable, IHasCustomCardTraits
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
        bool checkEnemy = upgrade != Upgrade.B;
        List<int> overlaps = [];
        foreach (int i in c.stuff.Keys)
        {
            if (s.ship.GetPartAtWorldX(i) is Part p && p.type != PType.empty)
            {
                overlaps.Add(i);
                continue;
            }

            if (checkEnemy && c.otherShip.GetPartAtWorldX(i) is Part cp && cp.type != PType.empty)
            {
                overlaps.Add(i);
            }
        }
        int x = overlaps.Count;

        return
        [
            new AStatus
            {
                status = Status.tempShield,
                statusAmount = x,
                targetPlayer = true
            }
        ];
    }


    public override CardData GetData(State state)
    {
        CardData cd = upgrade switch
        {
            _ => new CardData
            {
                cost = 0,
                retain = true
            }
        };

        cd.description = ModEntry.Instance.Localizations.Localize(["Kodijen", "card", rare.ToString(), MethodBase.GetCurrentMethod()!.DeclaringType!.Name, "desc"
            ], new
            {
                what = ModEntry.Instance.Localizations.Localize(["Kodijen", "card", rare.ToString(), MethodBase.GetCurrentMethod()!.DeclaringType!.Name, upgrade == Upgrade.B? "bee" : "base"])
            });
        return cd;
    }

    public IReadOnlySet<ICardTraitEntry> GetInnateTraits(State state)
    {
        return upgrade == Upgrade.B ? [] : new HashSet<ICardTraitEntry> { ModEntry.Instance.KokoroApi.V2.Limited.Trait };
    }

}