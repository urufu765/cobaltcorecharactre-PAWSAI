using System.Collections.Generic;
using System.Reflection;
using Nanoray.PluginManager;
using Nickel;

namespace Weth.Cards;

public class BilkBase : TarmaucCard, IRegisterable
{
    public override TarmaucTheme CardTheme { get; set; } = TarmaucTheme.Base;

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
            Name = ModEntry.Instance.AnyLocalizations.Bind(["Eriska", "card", "token", "Bilk", "name"]).Localize,
            //Art = ModEntry.RegisterSprite(package, $"assets/card/5/{MethodBase.GetCurrentMethod()!.DeclaringType!.Name}.png").Sprite
        });
    }


    public override List<CardAction> GetActions(State s, Combat c)
    {
        return upgrade switch
        {
            Upgrade.A =>
            [
                new AMove
                {
                    dir = 5,
                    isTeleport = true
                },
                new AStatus
                {
                    status = Status.heat,
                    statusAmount = 1,
                    targetPlayer = true
                }
            ],
            _ =>
            [
                new AMove
                {
                    dir = 3,
                    isTeleport = true
                },
                new AStatus
                {
                    status = Status.heat,
                    statusAmount = 1,
                    targetPlayer = true
                }
            ]
        };
    }

    public override CardData GetPreData(State state)
    {
        return upgrade switch
        {
            Upgrade.B => new CardData
            {
                cost = 1,
                retain = true,
                infinite = true
            },
            _ => new CardData
            {
                cost = 1,
                retain = true,
            }
        };
    }
}