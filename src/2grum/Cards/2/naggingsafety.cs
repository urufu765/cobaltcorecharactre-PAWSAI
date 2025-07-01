using System.Collections.Generic;
using System.Reflection;
using Nanoray.PluginManager;
using Nickel;

namespace Starhunters.Bruno.Cards;

/// <summary>
/// Bruno.
/// </summary>
public class NaggingSafety : Card, IRegisterable, IHasCustomCardTraits
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
                new AStatus
                {
                    status = Status.tempShield,
                    statusAmount = x,
                    targetPlayer = true,
                    xHint = 1
                },
                new AEnergy
                {
                    changeAmount = -x,
                    xHint = -1
                },
                new AStatus
                {
                    status = ModEntry.Instance.Status_Mitigate.Status,
                    statusAmount = 2,
                    targetPlayer = true
                }
            ],
            Upgrade.A =>
            [
                new AVariableHint
                {
                    status = ModEntry.Instance.Status_SlowBurn.Status
                },
                new AStatus
                {
                    status = Status.tempShield,
                    statusAmount = x,
                    targetPlayer = true,
                    xHint = 1
                },
                ModEntry.Instance.KokoroApi.V2.EnergyAsStatus.MakeStatusAction(1, AStatusMode.Set).AsCardAction
            ],
            _ =>
            [
                new AVariableHint
                {
                    status = ModEntry.Instance.Status_SlowBurn.Status
                },
                new AStatus
                {
                    status = Status.tempShield,
                    statusAmount = x,
                    targetPlayer = true,
                    xHint = 1
                },
                new AEnergy
                {
                    changeAmount = -x,
                    xHint = -1
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
                cost = 1,
            },
            _ => new CardData
            {
                cost = 0,
            }
        };
    }

    public IReadOnlySet<ICardTraitEntry> GetInnateTraits(State state)
    {
        return new HashSet<ICardTraitEntry>{ModEntry.Instance.KokoroApi.V2.Fleeting.Trait};
    }
}