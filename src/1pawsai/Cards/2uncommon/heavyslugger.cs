using System.Collections.Generic;
using System.Reflection;
using Nanoray.PluginManager;
using Nickel;

namespace Starhunters.Pawsai.Cards;

/// <summary>
/// Pawsai.
/// </summary>
public class HeavySlugger : Card, IRegisterable
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
            Upgrade.B =>
            [
                new AAttack
                {
                    damage = GetDmg(s, 3),
                    status = Status.tempShield,
                    statusAmount = 2,
                    piercing = true
                },
                new AStatus
                {
                    status = ModEntry.Instance.Status_Regeneration.Status,
                    statusAmount = 2,
                    targetPlayer = true,
                }
            ],
            Upgrade.A => 
            [
                new AAttack
                {
                    damage = GetDmg(s, 5),
                    status = Status.tempShield,
                    statusAmount = 4,
                    piercing = true
                }
            ],
            _ => 
            [
                new AAttack
                {
                    damage = GetDmg(s, 3),
                    status = Status.tempShield,
                    statusAmount = 2,
                    piercing = true
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
                cost = 2,
            }
        };
    }
}