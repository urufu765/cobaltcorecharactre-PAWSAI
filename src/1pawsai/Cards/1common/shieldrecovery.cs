using System;
using System.Collections.Generic;
using System.Reflection;
using Nanoray.PluginManager;
using Nickel;

namespace Starhunters.Pawsai.Cards;

/// <summary>
/// Pawsai.
/// </summary>
public class ShieldRecovery : Card, IRegisterable
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
        return upgrade switch
        {
            Upgrade.B =>
            [
                new AStatus
                {
                    status = Status.shield,
                    statusAmount = 3,
                    targetPlayer = true
                },
                new AStatus
                {
                    status = ModEntry.Instance.Status_Regeneration.Status,
                    statusAmount = 6,
                    targetPlayer = true
                }
            ],
            Upgrade.A =>
            [
                new AStatus
                {
                    status = Status.shield,
                    statusAmount = 2,
                    targetPlayer = true
                },
                new AStatus
                {
                    status = ModEntry.Instance.Status_Regeneration.Status,
                    statusAmount = 4,
                    targetPlayer = true
                }
            ],
            _ =>
            [
                new AStatus
                {
                    status = Status.shield,
                    statusAmount = 1,
                    targetPlayer = true
                },
                new AStatus
                {
                    status = ModEntry.Instance.Status_Regeneration.Status,
                    statusAmount = 3,
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
                exhaust = true
            },
            _ => new CardData
            {
                cost = 2,
            }
        };
    }
}