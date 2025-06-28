using System.Collections.Generic;
using System.Reflection;
using Nanoray.PluginManager;
using Nickel;

namespace Starhunters.Pawsai.Cards;

/// <summary>
/// Pawsai.
/// </summary>
public class ThrustRedirect : Card, IRegisterable
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
                upgradesTo = [Upgrade.A, Upgrade.B]
            },
            Name = ModEntry.Instance.AnyLocalizations.Bind(["Pawsai", "card", rare.ToString(), MethodBase.GetCurrentMethod()!.DeclaringType!.Name, "name"]).Localize,
            Art = ModEntry.RegisterSprite(package, $"assets/card/pawsai/{rare}/{MethodBase.GetCurrentMethod()!.DeclaringType!.Name}.png").Sprite
        });
    }


    public override List<CardAction> GetActions(State s, Combat c)
    {
        int x = s.ship.Get(Status.hermes) + s.ship.Get(Status.evade);
        return upgrade switch
        {
            Upgrade.B =>
            [
                new AVariableHint
                {
                    status = Status.hermes,
                    secondStatus = Status.evade
                },
                new AStatus
                {
                    status = Status.shield,
                    statusAmount = x,
                    targetPlayer = true,
                    xHint = 1
                },
                new AStatus
                {
                    status = ModEntry.Instance.Status_Regeneration.Status,
                    statusAmount = x * 2,
                    targetPlayer = true,
                    xHint = 2
                },
                new AStatus
                {
                    status = Status.hermes,
                    statusAmount = 0,
                    mode = AStatusMode.Set,
                    targetPlayer = true
                },
                new AStatus
                {
                    status = Status.evade,
                    statusAmount = 0,
                    mode = AStatusMode.Set,
                    targetPlayer = true
                }
            ],
            _ =>
            [
                new AVariableHint
                {
                    status = Status.hermes,
                    secondStatus = Status.evade
                },
                new AStatus
                {
                    status = Status.shield,
                    statusAmount = x,
                    targetPlayer = true,
                    xHint = 1
                },
                new AStatus
                {
                    status = Status.hermes,
                    statusAmount = 0,
                    mode = AStatusMode.Set,
                    targetPlayer = true
                },
                new AStatus
                {
                    status = Status.evade,
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
            Upgrade.B => new CardData
            {
                cost = 2,
            },
            Upgrade.A => new CardData
            {
                cost = 1,
                retain = true
            },
            _ => new CardData
            {
                cost = 1,
            }
        };
    }
}