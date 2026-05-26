using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Nanoray.PluginManager;
using Nickel;

namespace Weth.Cards;

public class JogWheel : TarmaucCard, IRegisterable, IHasCustomCardTraits
{
    public const Rarity rare = Rarity.uncommon;
    public static void Register(IPluginPackage<IModManifest> package, IModHelper helper)
    {
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
            //Art = ModEntry.RegisterSprite(package, $"assets/card/6/{MethodBase.GetCurrentMethod()!.DeclaringType!.Name}.png").Sprite
        });
        ModEntry.Instance.KokoroApi.V2.Limited.SetBaseLimitedUses(ice.UniqueName, Upgrade.None, 3);
        ModEntry.Instance.KokoroApi.V2.Limited.SetBaseLimitedUses(ice.UniqueName, Upgrade.A, 3);
        ModEntry.Instance.KokoroApi.V2.Limited.SetBaseLimitedUses(ice.UniqueName, Upgrade.B, 3);
    }


    public override List<CardAction> GetActions(State s, Combat c)
    {
        return upgrade switch
        {
            _ =>
            [
                new AStatus
                {
                    status = Status.evade,
                    statusAmount = 2,
                    targetPlayer = true
                },
                new AStatus
                {
                    status = Status.heat,
                    statusAmount = 1,
                    targetPlayer = true
                },
            ]
        };
    }

    public override CardData GetPreData(State state)
    {
        return upgrade switch
        {
            Upgrade.A => new CardData
            {
                cost = 0,
                recycle = true,
            },
            _ => new CardData
            {
                cost = 1,
                recycle = true,
            }
        };
    }


    public IReadOnlySet<ICardTraitEntry> GetInnateTraits(State state)
    {
        return new HashSet<ICardTraitEntry> {
            ModEntry.Instance.KokoroApi.V2.Limited.Trait,
            ModEntry.Instance.KokoroApi.V2.Fleeting.Trait
        };
    }

}