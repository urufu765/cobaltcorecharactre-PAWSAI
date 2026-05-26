using System.Collections.Generic;
using System.Reflection;
using Nanoray.PluginManager;
using Nickel;

namespace Weth.Cards;

public class DumperBase : TarmaucCard, IRegisterable
{
    public override TarmaucTheme CardTheme { get; set; } = TarmaucTheme.Base;

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
            Name = ModEntry.Instance.AnyLocalizations.Bind(["Eriska", "card", "token", "Dumper", "name"]).Localize,
            //Art = ModEntry.RegisterSprite(package, $"assets/card/5/{MethodBase.GetCurrentMethod()!.DeclaringType!.Name}.png").Sprite
        });
    }


    public override List<CardAction> GetActions(State s, Combat c)
    {
        int h = s.ship.Get(Status.heat);
        return upgrade switch
        {
            Upgrade.B =>
            [
                new AVariableHint
                {
                    status = Status.heat
                },
                new AStatus
                {
                    status = ModEntry.Instance.BurnStatus.Status,
                    statusAmount = h,
                    xHint = 1,
                    targetPlayer = false
                },
                new AHurt
                {
                    hurtAmount = h,
                    xHint = 1,
                    hurtShieldsFirst = true,
                    targetPlayer = true
                },
                new AStatus
                {
                    status = Status.heat,
                    statusAmount = 0,
                    targetPlayer = true,
                    mode = AStatusMode.Set
                }
            ],
            Upgrade.A =>
            [
                new AVariableHint
                {
                    status = Status.heat
                },
                new AStatus
                {
                    status = Status.heat,
                    statusAmount = h,
                    xHint = 1,
                    targetPlayer = false
                },
                new AStatus
                {
                    status = Status.heat,
                    statusAmount = 0,
                    targetPlayer = true,
                    mode = AStatusMode.Set
                }
            ],
            _ =>
            [
                new AVariableHint
                {
                    status = Status.heat
                },
                new AStatus
                {
                    status = Status.heat,
                    statusAmount = h,
                    xHint = 1,
                    targetPlayer = false
                },
                new AHurt
                {
                    hurtAmount = 1,
                    hurtShieldsFirst = true,
                    targetPlayer = true
                },
                new AStatus
                {
                    status = Status.heat,
                    statusAmount = 0,
                    targetPlayer = true,
                    mode = AStatusMode.Set
                }
            ]
        };
    }

    public override CardData GetPreData(State state)
    {
        return upgrade switch
        {
            Upgrade.A => new CardData
            {
                cost = 2,
                retain = true
            },
            _ => new CardData
            {
                cost = 2
            }
        };
    }
}