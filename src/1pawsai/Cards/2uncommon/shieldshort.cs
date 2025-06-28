using System.Collections.Generic;
using System.Reflection;
using Nanoray.PluginManager;
using Nickel;

namespace Starhunters.Pawsai.Cards;

/// <summary>
/// Pawsai.
/// </summary>
public class ShieldShort : Card, IRegisterable
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
        int x = s.ship.Get(Status.shield);
        return upgrade switch
        {
            _ =>
            [
                new AVariableHint
                {
                    status = Status.shield
                },
                new AStatus
                {
                    status = Status.tempShield,
                    statusAmount = x * 2,
                    targetPlayer = true,
                    xHint = 2
                },
                new AStatus
                {
                    status = Status.shield,
                    statusAmount = 0,
                    targetPlayer = true,
                    mode = AStatusMode.Set
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
                retain = true
            },
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