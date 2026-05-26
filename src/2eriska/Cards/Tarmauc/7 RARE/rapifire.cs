using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Nanoray.PluginManager;
using Nickel;

namespace Weth.Cards;

public class Rapiburner : TarmaucCard, IRegisterable, IHasCustomCardTraits
{
    public int Uses { get; set; } = 0;

    public int LastTurn { get; set; } = 0;

    public const Rarity rare = Rarity.rare;
    public static string CallMe => MethodBase.GetCurrentMethod()!.DeclaringType!.Name;

    public static void Register(IPluginPackage<IModManifest> package, IModHelper helper)
    {
        helper.Content.Cards.RegisterCard(new CardConfiguration
        {
            CardType = MethodBase.GetCurrentMethod()!.DeclaringType!,
            Meta = new CardMeta
            {
                rarity = rare,
                upgradesTo = [Upgrade.A, Upgrade.B],
                deck = ModEntry.Instance.EriskaDeck.Deck
            },
            Name = ModEntry.Instance.AnyLocalizations.Bind(["Eriska", "card", rare.ToString(), CallMe, "name"]).Localize,
            //Art = ModEntry.RegisterSprite(package, $"assets/card/7/{CallMe}.png").Sprite
        });
    }


    public override void OnDraw(State s, Combat c)
    {
        Uses = 0;
        LastTurn = c.turn;
    }

    public override void AfterWasPlayed(State state, Combat c)
    {
        Uses++;
    }


    public override List<CardAction> GetActions(State s, Combat c)
    {
        if (LastTurn != c.turn)
        {
            Uses = 0;
            LastTurn = c.turn;
        }
        return
        [
            .. Enumerable.Range(0, Uses + 1)
            .Select(_ => new AAttack
            {
                damage = GetDmg(s, upgrade == Upgrade.A? 1 : 0),
                fast = true,
                status = upgrade == Upgrade.B? Status_burn : Status.heat,
                statusAmount = 1
            })
        ];
    }

    public override CardData GetPreData(State state)
    {
        bool inCombat = state.route is Combat;
        return new CardData
        {
            cost = 1,
            infinite = true,
            description = ModEntry.Instance.Localizations.Localize(["Eriska", "card", rare.ToString(), CallMe, "desc"], new
            {
                mult = inCombat ? Uses + 1 : 1,
                modifier = upgrade == Upgrade.B ? ModEntry.Instance.Localizations.Localize(["Eriska", "card", rare.ToString(), CallMe, "burn"]) : ModEntry.Instance.Localizations.Localize(["Eriska", "card", rare.ToString(), CallMe, "heat"]),
                attack = GetDmg(state, upgrade == Upgrade.A ? 1 : 0),
            }),
        };
    }

    public IReadOnlySet<ICardTraitEntry> GetInnateTraits(State state)
    {
        return upgrade == Upgrade.B ? new HashSet<ICardTraitEntry> { ModEntry.Instance.KokoroApi.V2.Fleeting.Trait } : [ModEntry.Instance.KokoroApi.V2.Fleeting.Trait, ModEntry.Instance.KokoroApi.V2.Heavy.Trait];
    }

}