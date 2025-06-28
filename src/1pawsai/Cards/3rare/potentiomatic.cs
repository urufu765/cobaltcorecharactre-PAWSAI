using System.Collections.Generic;
using System.Reflection;
using Nanoray.PluginManager;
using Nickel;

namespace Starhunters.Pawsai.Cards;

/// <summary>
/// Pawsai.
/// </summary>
public class Potentiomatic : Card, IRegisterable, IHasCustomCardTraits
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
            Upgrade.B =>
            [
                new AVariableHint
                {
                    status = Status.shield
                },
                new AStatus
                {
                    status = Status.maxShield,
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
            _ =>
            [
                new AVariableHint
                {
                    status = Status.shield
                },
                new AStatus
                {
                    status = Status.maxShield,
                    statusAmount = x,
                    targetPlayer = true,
                    xHint = 1
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
            Upgrade.A => new CardData
            {
                cost = 2,
                exhaust = true,
                retain = true
            },
            _ => new CardData
            {
                cost = 2,
                exhaust = true,
            }
        };
    }

    public IReadOnlySet<ICardTraitEntry> GetInnateTraits(State state)
    {
        return upgrade == Upgrade.B? new HashSet<ICardTraitEntry>{ModEntry.Instance.KokoroApi.V2.Fleeting.Trait} : [];
    }
}