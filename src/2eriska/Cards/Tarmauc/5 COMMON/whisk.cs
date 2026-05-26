using System.Collections.Generic;
using System.Reflection;
using Nanoray.PluginManager;
using Nickel;

namespace Weth.Cards;

public class Whisk : TarmaucCard, IRegisterable
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
                deck = ModEntry.Instance.EriskaDeck.Deck
            },
            Name = ModEntry.Instance.AnyLocalizations.Bind(["Eriska", "card", rare.ToString(), MethodBase.GetCurrentMethod()!.DeclaringType!.Name, "name"]).Localize,
            //Art = ModEntry.RegisterSprite(package, $"assets/card/5/{MethodBase.GetCurrentMethod()!.DeclaringType!.Name}.png").Sprite
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
                    dir = 2,
                    targetPlayer = true,
                    isTeleport = true
                },
                new AStatus
                {
                    status = Status_burn,
                    statusAmount = 2,
                    targetPlayer = false
                },
                new AStatus
                {
                    status = Status.heat,
                    statusAmount = 1,
                    targetPlayer = true
                },
            ],
            Upgrade.A =>
            [
                new AMove
                {
                    dir = 1,
                    targetPlayer = true,
                    isTeleport = true
                },
                new AStatus
                {
                    status = Status_burn,
                    statusAmount = 1,
                    targetPlayer = false
                },
            ],
            _ =>
            [
                new AMove
                {
                    dir = 1,
                    targetPlayer = true,
                    isTeleport = true
                },
                new AStatus
                {
                    status = Status_burn,
                    statusAmount = 1,
                    targetPlayer = false
                },
                new AStatus
                {
                    status = Status.heat,
                    statusAmount = 1,
                    targetPlayer = true
                },
            ]
        };
    }

    public override CardData GetPreData(State state)
    {
        return upgrade switch
        {
            _ => new CardData
            {
                cost = 0
            }
        };
    }
}