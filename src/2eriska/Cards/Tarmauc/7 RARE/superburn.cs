using System.Collections.Generic;
using System.Reflection;
using Nanoray.PluginManager;
using Nickel;

namespace Weth.Cards;

public class SuperBurn : TarmaucCard, IRegisterable, IHasCustomCardTraits
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
        ModEntry.Instance.KokoroApi.V2.Limited.SetBaseLimitedUses(ice.UniqueName, Upgrade.None, 2);
        ModEntry.Instance.KokoroApi.V2.Limited.SetBaseLimitedUses(ice.UniqueName, Upgrade.A, 2);
        ModEntry.Instance.KokoroApi.V2.Limited.SetBaseLimitedUses(ice.UniqueName, Upgrade.B, 5);
    }


    public override List<CardAction> GetActions(State s, Combat c)
    {
        int h = c.otherShip.Get(Status.heat);
        return upgrade switch
        {
            Upgrade.A =>
            [
                ModEntry.Instance.KokoroApi.V2.VariableHintTargetPlayerTargetPlayer.MakeVariableHint(
                    new AVariableHint
                    {
                        status = Status.heat
                    }
                ).SetTargetPlayer(false).AsCardAction,
                new AStatus
                {
                    status = Status_burn,
                    statusAmount = h,
                    targetPlayer = false,
                    xHint = 1
                }
            ],
            _ =>
            [
                ModEntry.Instance.KokoroApi.V2.VariableHintTargetPlayerTargetPlayer.MakeVariableHint(
                    new AVariableHint
                    {
                        status = Status.heat
                    }
                ).SetTargetPlayer(false).AsCardAction,
                new AStatus
                {
                    status = Status_burn,
                    statusAmount = h,
                    targetPlayer = false,
                    xHint = 1
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
            _ => new CardData
            {
                cost = 1,
            }
        };
    }

    public IReadOnlySet<ICardTraitEntry> GetInnateTraits(State state)
    {
        return upgrade == Upgrade.A ? new HashSet<ICardTraitEntry> { ModEntry.Instance.KokoroApi.V2.Limited.Trait, ModEntry.Instance.KokoroApi.V2.Heavy.Trait } : [ModEntry.Instance.KokoroApi.V2.Limited.Trait];
    }
}