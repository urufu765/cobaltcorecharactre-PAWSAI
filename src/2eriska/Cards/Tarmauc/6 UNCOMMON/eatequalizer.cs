using System.Collections.Generic;
using System.Reflection;
using Nanoray.PluginManager;
using Nickel;

namespace Weth.Cards;

public class HeatEqualizer : TarmaucCard, IRegisterable, IHasCustomCardTraits
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
        int h = flipped ? s.ship.Get(Status.heat) : c.otherShip.Get(Status.heat);
        return upgrade switch
        {
            _ =>
            [
                ModEntry.Instance.KokoroApi.V2.VariableHintTargetPlayerTargetPlayer.MakeVariableHint(
                    new AVariableHint
                    {
                        status = Status.heat,
                        disabled = flipped
                    }
                ).SetTargetPlayer(false).AsCardAction,
                new AStatus
                {
                    status = Status.heat,
                    statusAmount = h,
                    targetPlayer = true,
                    disabled = flipped,
                    xHint = 1
                },
                new ADummyAction(),
                new AVariableHint
                {
                    status = Status.heat,
                    disabled = !flipped
                },
                new AStatus
                {
                    status = Status.heat,
                    statusAmount = h,
                    targetPlayer = false,
                    disabled = !flipped,
                    xHint = 1
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
                retain = true
            },
            Upgrade.A => new CardData
            {
                cost = 0,
                floppable = true,
            },
            _ => new CardData
            {
                cost = 1,
                floppable = true,
            }
        };
    }

    public IReadOnlySet<ICardTraitEntry> GetInnateTraits(State state)
    {
        return upgrade == Upgrade.B ? new HashSet<ICardTraitEntry> { ModEntry.Instance.KokoroApi.V2.Fleeting.Trait } : [];
    }
}