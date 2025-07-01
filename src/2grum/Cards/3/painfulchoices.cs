using System.Collections.Generic;
using System.Reflection;
using Nanoray.PluginManager;
using Nickel;

namespace Starhunters.Bruno.Cards;

/// <summary>
/// Bruno.
/// </summary>
public class PainfulChoices : Card, IRegisterable, IHasCustomCardTraits
{
    public static void Register(IPluginPackage<IModManifest> package, IModHelper helper)
    {
        Rarity rare = Rarity.rare;
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
        int x = s.ship.Get(ModEntry.Instance.Status_SlowBurn.Status);
        return upgrade switch
        {
            Upgrade.B =>
            [
                new AVariableHint
                {
                    status = ModEntry.Instance.Status_SlowBurn.Status
                },
                new AHurt
                {
                    hurtAmount = x,
                    xHint = 1,
                    hurtShieldsFirst = true,
                    targetPlayer = true
                },
                new ADrawCard
                {
                    count = x,
                    xHint = 1
                },
                new AStatus
                {
                    status = ModEntry.Instance.Status_Mitigate.Status,
                    statusAmount = 3,
                    targetPlayer = true
                }
            ],
            Upgrade.A =>
            [
                new AVariableHint
                {
                    status = ModEntry.Instance.Status_SlowBurn.Status
                },
                new AHurt
                {
                    hurtAmount = x,
                    xHint = 1,
                    hurtShieldsFirst = true,
                    targetPlayer = true
                },
                new ADrawCard
                {
                    count = x,
                    xHint = 1
                }
            ],
            _ =>
            [
                new AVariableHint
                {
                    status = ModEntry.Instance.Status_SlowBurn.Status
                },
                new AHurt
                {
                    hurtAmount = x,
                    xHint = 1,
                    hurtShieldsFirst = true,
                    targetPlayer = true
                },
                new ADrawCard
                {
                    count = x,
                    xHint = 1
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
                cost = 0,
            }
        };
    }

    public IReadOnlySet<ICardTraitEntry> GetInnateTraits(State state)
    {
        return upgrade == Upgrade.A? [] : new HashSet<ICardTraitEntry>{ModEntry.Instance.KokoroApi.V2.Fleeting.Trait};
    }
}