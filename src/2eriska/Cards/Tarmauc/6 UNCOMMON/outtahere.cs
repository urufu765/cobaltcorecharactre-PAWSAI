using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Nanoray.PluginManager;
using Nickel;

namespace Weth.Cards;

public class OuttaHere : TarmaucCard, IRegisterable, IHasCustomCardTraits
{
    public const Rarity rare = Rarity.uncommon;
    public static string CallMe => MethodBase.GetCurrentMethod()!.DeclaringType!.Name;
    public static void Register(IPluginPackage<IModManifest> package, IModHelper helper)
    {
        ICardEntry ice = helper.Content.Cards.RegisterCard(new CardConfiguration
        {
            CardType = MethodBase.GetCurrentMethod()!.DeclaringType!,
            Meta = new CardMeta
            {
                rarity = rare,
                upgradesTo = [Upgrade.A, Upgrade.B],
                deck = ModEntry.Instance.EriskaDeck.Deck
            },
            Name = ModEntry.Instance.AnyLocalizations.Bind(["Eriska", "card", rare.ToString(), CallMe, "name"]).Localize,
            //Art = ModEntry.RegisterSprite(package, $"assets/card/6/{CallMe}.png").Sprite
        });
        ModEntry.Instance.KokoroApi.V2.Limited.SetBaseLimitedUses(ice.UniqueName, Upgrade.None, 2);
        ModEntry.Instance.KokoroApi.V2.Limited.SetBaseLimitedUses(ice.UniqueName, Upgrade.A, 2);
        ModEntry.Instance.KokoroApi.V2.Limited.SetBaseLimitedUses(ice.UniqueName, Upgrade.B, 2);
    }


    public override List<CardAction> GetActions(State s, Combat c)
    {
        return upgrade switch
        {
            Upgrade.B =>
            [
                new AMove
                {
                    dir = 2 * GetEnemyIntentCount(c.otherShip),
                    targetPlayer = true,
                    isTeleport = true
                },
                new AStatus
                {
                    status = Status.heat,
                    statusAmount = 1,
                    targetPlayer = true
                },
            ],
            _ =>
            [
                new AMove
                {
                    dir = 1 + GetEnemyIntentCount(c.otherShip),
                    targetPlayer = true,
                    isTeleport = true
                },
                new AStatus
                {
                    status = Status.heat,
                    statusAmount = 1,
                    targetPlayer = true
                },
            ]
        };
    }

    public override CardData GetPreData(State state)
    {
        bool inCombat = false;
        int moveExtra = 0;
        if (state.route is Combat c && c.otherShip is not null && c.otherShip.parts.Count > 0)
        {
            inCombat = true;
            moveExtra = GetEnemyIntentCount(c.otherShip);
        }

        return new CardData
        {
            cost = 2,
            description = ModEntry.Instance.Localizations.Localize(["Eriska", "card", rare.ToString(), CallMe, "desc"], new
            {
                amount = upgrade == Upgrade.B ? 0 : 1,
                direction = flipped ? ModEntry.Instance.Localizations.Localize(["Eriska", "card", rare.ToString(), CallMe, "l"]) : ModEntry.Instance.Localizations.Localize(["Eriska", "card", rare.ToString(), CallMe, "r"]),
                addition = upgrade == Upgrade.B ? 2 : 1,
                extra = inCombat ? ModEntry.Instance.Localizations.Localize(["Eriska", "card", rare.ToString(), CallMe, "plus"], new
                {
                    moar = moveExtra
                }
                ) : ""
            }),
            flippable = upgrade == Upgrade.A
        };
    }

    private static int GetEnemyIntentCount(Ship ship)
    {
        if (ship is null) return 0;
        if (ship.parts.Count == 0) return 0;
        return ship.parts.Count(p => p.intent is not null);
    }

    public IReadOnlySet<ICardTraitEntry> GetInnateTraits(State state)
    {
        return new HashSet<ICardTraitEntry> { ModEntry.Instance.KokoroApi.V2.Limited.Trait };
    }

}