using System.Collections.Generic;
using System.Reflection;
using Nanoray.PluginManager;
using Nickel;

namespace Weth.Cards;

public class BlastTime : TarmaucCard, IRegisterable
{
    public static void Register(IPluginPackage<IModManifest> package, IModHelper helper)
    {
        Rarity rare = Rarity.rare;
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
            //Art = ModEntry.RegisterSprite(package, $"assets/card/7/{MethodBase.GetCurrentMethod()!.DeclaringType!.Name}.png").Sprite
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
                    damage = GetDmg(s, 0),
                    status = Status.heat,
                    statusAmount = 1
                },
                new AMove
                {
                    dir = 1,
                    isTeleport = true,
                    targetPlayer = true
                },
                new AAttack
                {
                    damage = GetDmg(s, 0),
                    status = Status_burn,
                    statusAmount = 2
                },
                new AMove
                {
                    dir = 1,
                    isTeleport = true,
                    targetPlayer = true
                },
                new AAttack
                {
                    damage = GetDmg(s, 0),
                    status = Status_blister,
                    statusAmount = 3
                }
            ],
            _ =>
            [
                new AAttack
                {
                    damage = GetDmg(s, 0),
                    status = Status.heat,
                    statusAmount = 1
                },
                new AMove
                {
                    dir = 1,
                    isTeleport = true,
                    targetPlayer = true
                },
                new AAttack
                {
                    damage = GetDmg(s, 0),
                    status = Status_burn,
                    statusAmount = 1
                },
                new AMove
                {
                    dir = 1,
                    isTeleport = true,
                    targetPlayer = true
                },
                new AAttack
                {
                    damage = GetDmg(s, 0),
                    status = Status_blister,
                    statusAmount = 1
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
                exhaust = true
            },
            Upgrade.A => new CardData
            {
                cost = 1,
                flippable = true
            },
            _ => new CardData
            {
                cost = 1,
            }
        };
    }
}