using System.Collections.Generic;
using System.Reflection;
using Nanoray.PluginManager;
using Nickel;

namespace Weth.Cards;

public class HeatBurst : TarmaucCard, IRegisterable, IHasCustomCardTraits
{
    public static void Register(IPluginPackage<IModManifest> package, IModHelper helper)
    {
        Rarity rare = Rarity.rare;
        ICardEntry ice = helper.Content.Cards.RegisterCard(new CardConfiguration
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
        ModEntry.Instance.KokoroApi.V2.Limited.SetBaseLimitedUses(ice.UniqueName, Upgrade.None, 3);
        ModEntry.Instance.KokoroApi.V2.Limited.SetBaseLimitedUses(ice.UniqueName, Upgrade.A, 5);
        ModEntry.Instance.KokoroApi.V2.Limited.SetBaseLimitedUses(ice.UniqueName, Upgrade.B, 2);
    }


    public override List<CardAction> GetActions(State s, Combat c)
    {
        int b = c.otherShip.Get(Status_burn) + c.otherShip.Get(Status_blister);
        return upgrade switch
        {
            Upgrade.B =>
            [
                ModEntry.Instance.KokoroApi.V2.VariableHintTargetPlayerTargetPlayer.MakeVariableHint(
                    new AVariableHint
                    {
                        status = Status_burn,
                        secondStatus = Status_blister
                    }
                ).SetTargetPlayer(false).AsCardAction,
                new AHurt
                {
                    hurtAmount = b * 3,
                    xHint = 3
                },
                new AStatus
                {
                    status = Status.heat,
                    statusAmount = b,
                    targetPlayer = false,
                    xHint = 1
                },
                new AStatus
                {
                    status = Status_burn,
                    statusAmount = 0,
                    targetPlayer = false,
                    mode = AStatusMode.Set
                },
                new AStatus
                {
                    status = Status_blister,
                    statusAmount = 0,
                    targetPlayer = false,
                    mode = AStatusMode.Set
                }
            ],
            _ =>
            [
                ModEntry.Instance.KokoroApi.V2.VariableHintTargetPlayerTargetPlayer.MakeVariableHint(
                    new AVariableHint
                    {
                        status = Status_burn,
                        secondStatus = Status_blister
                    }
                ).SetTargetPlayer(false).AsCardAction,
                new AHurt
                {
                    hurtAmount = b * 2,
                    xHint = 2
                },
                new AStatus
                {
                    status = Status.heat,
                    statusAmount = b,
                    targetPlayer = false,
                    xHint = 1
                },
                new AStatus
                {
                    status = Status_burn,
                    statusAmount = 0,
                    targetPlayer = false,
                    mode = AStatusMode.Set
                },
                new AStatus
                {
                    status = Status_blister,
                    statusAmount = 0,
                    targetPlayer = false,
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
                cost = 1,
            },
            _ => new CardData
            {
                cost = 2,
            }
        };
    }

    public IReadOnlySet<ICardTraitEntry> GetInnateTraits(State state)
    {
        return new HashSet<ICardTraitEntry> { ModEntry.Instance.KokoroApi.V2.Limited.Trait };
    }
}