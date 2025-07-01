using System.Collections.Generic;
using System.Reflection;
using Nanoray.PluginManager;
using Nickel;

namespace Starhunters.Bruno.Cards;

/// <summary>
/// Bruno.
/// </summary>
public class ForwardsConverter : Card, IRegisterable
{
    public static void Register(IPluginPackage<IModManifest> package, IModHelper helper)
    {
        Rarity rare = Rarity.common;
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
        int x = s.ship.Get(ModEntry.Instance.Status_Mitigate.Status);
        return upgrade switch
        {
            Upgrade.B =>
            [
                new AVariableHint
                {
                    status = ModEntry.Instance.Status_Mitigate.Status
                },
                new AStatus
                {
                    status = Status.tempShield,
                    statusAmount = x,
                    mode = AStatusMode.Set,
                    targetPlayer = true,
                    xHint = 1
                },
                new AStatus
                {
                    status = ModEntry.Instance.Status_Mitigate.Status,
                    statusAmount = 1,
                    mode = AStatusMode.Set,
                    targetPlayer = true
                }
            ],
            _ =>
            [
                new AVariableHint
                {
                    status = ModEntry.Instance.Status_Mitigate.Status
                },
                new AStatus
                {
                    status = Status.tempShield,
                    statusAmount = x,
                    mode = AStatusMode.Set,
                    targetPlayer = true,
                    xHint = 1
                },
                new AStatus
                {
                    status = ModEntry.Instance.Status_Mitigate.Status,
                    statusAmount = 0,
                    mode = AStatusMode.Set,
                    targetPlayer = true
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
                cost = 0,
            },
            _ => new CardData
            {
                cost = 1,
            }
        };
    }
}