using System.Collections.Generic;
using System.Reflection;
using Nanoray.PluginManager;
using Nickel;

namespace Starhunters.Pawsai.Cards;

/// <summary>
/// Pawsai.
/// </summary>
public class EnergizedMove : Card, IRegisterable
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
        int x = c.energy;
        return upgrade switch
        {
            Upgrade.B =>
            [
                ModEntry.Instance.KokoroApi.V2.EnergyAsStatus.MakeVariableHint().AsCardAction,
                new AMove
                {
                    dir = x,
                    targetPlayer = true,
                    xHint = 1
                }
            ],
            Upgrade.A =>
            [
                ModEntry.Instance.KokoroApi.V2.EnergyAsStatus.MakeVariableHint().AsCardAction,
                new AMove
                {
                    dir = -x,
                    targetPlayer = true,
                    xHint = 1
                },
                ModEntry.Instance.KokoroApi.V2.EnergyAsStatus.MakeStatusAction(0, AStatusMode.Set).AsCardAction
            ],
            _ =>
            [
                ModEntry.Instance.KokoroApi.V2.EnergyAsStatus.MakeVariableHint().AsCardAction,
                new AMove
                {
                    dir = x,
                    targetPlayer = true,
                    xHint = 1
                },
                ModEntry.Instance.KokoroApi.V2.EnergyAsStatus.MakeStatusAction(0, AStatusMode.Set).AsCardAction
            ],
        };
    }


    public override CardData GetData(State state)
    {
        return upgrade switch
        {
            Upgrade.B => new CardData
            {
                cost = 0,
                exhaust = true
            },
            Upgrade.A => new CardData
            {
                cost = 0,
                flippable = true
            },
            _ => new CardData
            {
                cost = 0,
            }
        };
    }
}