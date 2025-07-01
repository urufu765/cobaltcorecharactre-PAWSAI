using System.Collections.Generic;
using System.Reflection;
using Nanoray.PluginManager;
using Nickel;
using Starhunters.Bruno.Actions;

namespace Starhunters.Bruno.Cards;

/// <summary>
/// Bruno.
/// </summary>
public class HurtBurn : Card, IRegisterable
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
                deck = ModEntry.Instance.BrunoDeck.Deck
            },
            Name = ModEntry.Instance.AnyLocalizations.Bind(["Bruno", "card", rare.ToString(), MethodBase.GetCurrentMethod()!.DeclaringType!.Name, "name"]).Localize,
            Art = ModEntry.RegisterSprite(package, $"assets/card/bruno/{rare}/{MethodBase.GetCurrentMethod()!.DeclaringType!.Name}.png").Sprite
        });
    }


    public override List<CardAction> GetActions(State s, Combat c)
    {
        int x = s.ship.Get(ModEntry.Instance.Status_SlowBurn.Status);
        return upgrade switch
        {
            Upgrade.B =>
            [
                new AVariableHint
                {
                    status = ModEntry.Instance.Status_SlowBurn.Status
                },
                new ABreachAttack
                {
                    damage = GetDmg(s, x),
                    xHint = 1
                },
                new AStatus
                {
                    status = ModEntry.Instance.Status_SlowBurn.Status,
                    statusAmount = 0,
                    mode = AStatusMode.Set,
                    targetPlayer = true
                },
                new AEndTurn(),
            ],
            Upgrade.A =>
            [
                new AVariableHint
                {
                    status = ModEntry.Instance.Status_SlowBurn.Status
                },
                new ABreachAttack
                {
                    damage = GetDmg(s, x),
                    xHint = 1
                },
            ],
            _ =>
            [
                new AVariableHint
                {
                    status = ModEntry.Instance.Status_SlowBurn.Status
                },
                new ABreachAttack
                {
                    damage = GetDmg(s, x),
                    xHint = 1
                },
                new AEndTurn(),
            ],
        };
    }


    public override CardData GetData(State state)
    {
        return upgrade switch
        {
            Upgrade.B => new CardData
            {
                cost = 4,
                exhaust = true
            },
            _ => new CardData
            {
                cost = 3,
                exhaust = true
            }
        };
    }
}