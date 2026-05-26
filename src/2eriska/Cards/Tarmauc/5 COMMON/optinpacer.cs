using System.Collections.Generic;
using System.Reflection;
using Nanoray.PluginManager;
using Nickel;

namespace Weth.Cards;

public class OptInPacer : TarmaucCard, IRegisterable
{
    public static Spr fourSlot;
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
        fourSlot = StableSpr.cards_colorless;
    }


    public override List<CardAction> GetActions(State s, Combat c)
    {
        return upgrade switch
        {
            Upgrade.B =>
            [
                new AStatus
                {
                    status = Status.evade,
                    statusAmount = 2,
                    targetPlayer = true,
                    disabled = flipped
                },
                new ADummyAction(),
                new AStatus
                {
                    status = Status.hermes,
                    statusAmount = 1,
                    targetPlayer = true,
                    mode = AStatusMode.Set,
                    disabled = !flipped
                },
                new AStatus
                {
                    status = Status.heat,
                    statusAmount = 2,
                    targetPlayer = false,
                    disabled = !flipped
                }
            ],
            _ =>
            [
                new AStatus
                {
                    status = Status.evade,
                    statusAmount = 2,
                    targetPlayer = true,
                    disabled = flipped
                },
                new AStatus
                {
                    status = Status.heat,
                    statusAmount = 1,
                    targetPlayer = true,
                    disabled = flipped
                },
                new ADummyAction(),
                new AStatus
                {
                    status = Status.hermes,
                    statusAmount = 1,
                    targetPlayer = true,
                    mode = AStatusMode.Set,
                    disabled = !flipped
                },
                new AStatus
                {
                    status = Status.heat,
                    statusAmount = 1,
                    targetPlayer = false,
                    disabled = !flipped
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
                floppable = true,
                art = fourSlot
            },
            Upgrade.A => new CardData
            {
                cost = 1,
                floppable = true,
                retain = true
            },
            _ => new CardData
            {
                cost = 1,
                floppable = true
            }
        };
    }
}