using System.Collections.Generic;
using System.Reflection;
using Nanoray.PluginManager;
using Nickel;
using Starhunters.Pawsai.Actions;

namespace Starhunters.Pawsai.Cards;

/// <summary>
/// Pawsai.
/// </summary>
public class SpeedChaser : Card, IRegisterable
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
            Upgrade.A => 
            [
                new APivot
                {
                    amount = 2
                },
                new AStatus
                {
                    status = Status.hermes,
                    statusAmount = 1,
                    targetPlayer = true
                }
            ],
            _ => 
            [
                new APivot
                {
                    amount = 1
                },
                new AStatus
                {
                    status = Status.hermes,
                    statusAmount = 1,
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
                cost = 1,
                infinite = true,
            },
            _ => new CardData
            {
                cost = 1,
            }
        };
    }
}