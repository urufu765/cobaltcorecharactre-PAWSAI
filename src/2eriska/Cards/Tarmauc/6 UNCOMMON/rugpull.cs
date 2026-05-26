using System.Collections.Generic;
using System.Reflection;
using Nanoray.PluginManager;
using Nickel;

namespace Weth.Cards;

public class RugPull : TarmaucCard, IRegisterable
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
                deck = ModEntry.Instance.EriskaDeck.Deck
            },
            Name = ModEntry.Instance.AnyLocalizations.Bind(["Eriska", "card", rare.ToString(), MethodBase.GetCurrentMethod()!.DeclaringType!.Name, "name"]).Localize,
            //Art = ModEntry.RegisterSprite(package, $"assets/card/6/{MethodBase.GetCurrentMethod()!.DeclaringType!.Name}.png").Sprite
        });
    }


    public override List<CardAction> GetActions(State s, Combat c)
    {
        int n = c.hand.Count;
        return upgrade switch
        {
            Upgrade.B =>
            [

                new AVariableHint
                {
                    hand = true,
                    handAmount = n
                },
                new AStatus
                {
                    status = Status.evade,
                    statusAmount = n,
                    xHint = 1,
                    targetPlayer = true,
                },
                new AExhaustEntireHand(),
                new AEndTurn()
            ],
            _ =>
            [
                new AVariableHint
                {
                    hand = true,
                    handAmount = n
                },
                new AMove
                {
                    dir = n,
                    xHint = 1,
                    targetPlayer = true,
                    isTeleport = true
                },
                new AExhaustEntireHand()
            ]
        };
    }

    public override CardData GetPreData(State state)
    {
        return upgrade switch
        {
            Upgrade.A => new CardData
            {
                cost = 1,
                retain = true,
                exhaust = true
            },
            _ => new CardData
            {
                cost = 1,
                exhaust = true
            }
        };
    }
}