using System.Collections.Generic;
using System.Reflection;
using Nanoray.PluginManager;
using Nickel;

namespace Weth.Cards;

public class StutterShift : TarmaucCard, IRegisterable, IHasCustomCardTraits
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
                deck = ModEntry.Instance.EriskaDeck.Deck
            },
            Name = ModEntry.Instance.AnyLocalizations.Bind(["Eriska", "card", rare.ToString(), MethodBase.GetCurrentMethod()!.DeclaringType!.Name, "name"]).Localize,
            //Art = ModEntry.RegisterSprite(package, $"assets/card/6/{MethodBase.GetCurrentMethod()!.DeclaringType!.Name}.png").Sprite
        });
    }


    public override List<CardAction> GetActions(State s, Combat c)
    {
        return upgrade switch
        {
            Upgrade.B =>
            [
                new AMove
                {
                    dir = 1,
                    isTeleport = true,
                    targetPlayer = true,
                    isRandom = true
                },
                new AMove
                {
                    dir = 2,
                    isTeleport = true,
                    targetPlayer = true,
                    isRandom = true
                },
                new AMove
                {
                    dir = 3,
                    isTeleport = true,
                    targetPlayer = true,
                    isRandom = true
                },
                new AStatus
                {
                    status = Status.energyNextTurn,
                    statusAmount = 1,
                    targetPlayer = true,
                },
                new AStatus
                {
                    status = Status.heat,
                    statusAmount = 1,
                    targetPlayer = true
                }
            ],
            _ =>
            [
                new AMove
                {
                    dir = 1,
                    isTeleport = true,
                    targetPlayer = true,
                    isRandom = true
                },
                new AMove
                {
                    dir = 2,
                    isTeleport = true,
                    targetPlayer = true,
                    isRandom = true
                },
                new AStatus
                {
                    status = Status.energyNextTurn,
                    statusAmount = 1,
                    targetPlayer = true,
                },
                new AStatus
                {
                    status = Status.heat,
                    statusAmount = 1,
                    targetPlayer = true
                }
            ]
        };
    }

    public override CardData GetPreData(State state)
    {
        return upgrade switch
        {
            Upgrade.B => new CardData
            {
                cost = 1,
                recycle = true
            },
            Upgrade.A => new CardData
            {
                cost = 1,
                exhaust = true,
                retain = true
            },
            _ => new CardData
            {
                cost = 1,
                exhaust = true
            }
        };
    }

    public IReadOnlySet<ICardTraitEntry> GetInnateTraits(State state)
    {
        return upgrade == Upgrade.B ? new HashSet<ICardTraitEntry> { ModEntry.Instance.KokoroApi.V2.Fleeting.Trait } : [];
    }
}