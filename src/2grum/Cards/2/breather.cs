using System.Collections.Generic;
using System.Reflection;
using Nanoray.PluginManager;
using Nickel;

namespace Starhunters.Bruno.Cards;

/// <summary>
/// Bruno.
/// </summary>
public class Breather : Card, IRegisterable
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
        int x = c.energy;
        return upgrade switch
        {
            Upgrade.B =>
            [
                ModEntry.Instance.KokoroApi.V2.EnergyAsStatus.MakeVariableHint().AsCardAction,
                new AStatus
                {
                    status = ModEntry.Instance.Status_SlowBurn.Status,
                    statusAmount = x * -2,
                    targetPlayer = true,
                    xHint = -2
                },
                ModEntry.Instance.KokoroApi.V2.EnergyAsStatus.MakeStatusAction(0, AStatusMode.Set).AsCardAction,
                new AStatus
                {
                    status = ModEntry.Instance.Status_Mitigate.Status,
                    statusAmount = 2,
                    targetPlayer = true
                }
            ],
            Upgrade.A =>
            [
                ModEntry.Instance.KokoroApi.V2.EnergyAsStatus.MakeVariableHint().AsCardAction,
                new AStatus
                {
                    status = ModEntry.Instance.Status_SlowBurn.Status,
                    statusAmount = x * -3,
                    targetPlayer = true,
                    xHint = -3
                },
                ModEntry.Instance.KokoroApi.V2.EnergyAsStatus.MakeStatusAction(0, AStatusMode.Set).AsCardAction
            ],
            _ =>
            [
                ModEntry.Instance.KokoroApi.V2.EnergyAsStatus.MakeVariableHint().AsCardAction,
                new AStatus
                {
                    status = ModEntry.Instance.Status_SlowBurn.Status,
                    statusAmount = x * -2,
                    targetPlayer = true,
                    xHint = -2
                },
                ModEntry.Instance.KokoroApi.V2.EnergyAsStatus.MakeStatusAction(0, AStatusMode.Set).AsCardAction
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
                exhaust = true
            }
        };
    }
}