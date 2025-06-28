using System.Collections.Generic;
using System.Reflection;
using Nanoray.PluginManager;
using Nickel;

namespace Starhunters.Pawsai.Cards;

/// <summary>
/// Pawsai.
/// </summary>
public class EmergencyRetreat : Card, IRegisterable, IHasCustomCardTraits
{
    private static Rarity rare = Rarity.uncommon;
    public static void Register(IPluginPackage<IModManifest> package, IModHelper helper)
    {
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
        // TODO: do calculations here
        return upgrade switch
        {
            Upgrade.B =>
            [
            ],
            Upgrade.A =>
            [
            ],
            _ =>
            [
            ],
        };
    }


    public override CardData GetData(State state)
    {
        CardData cd = upgrade switch
        {
            Upgrade.B => new CardData
            {
                cost = 1,
                flippable = true,
            },
            Upgrade.A => new CardData
            {
                cost = 1,
                flippable = true,
                exhaust = true,
                retain = true
            },
            _ => new CardData
            {
                cost = 1,
                flippable = true,
                exhaust = true
            }
        };
        cd.description = ModEntry.Instance.Localizations.Localize(
            ["Pawsai", "card", rare.ToString(), MethodBase.GetCurrentMethod()!.DeclaringType!.Name, upgrade == Upgrade.B ? "descB" : "desc"],
            new List<string>() {
                ModEntry.Instance.Localizations.Localize(
                    ["Pawsai", "card", rare.ToString(), MethodBase.GetCurrentMethod()!.DeclaringType!.Name, flipped ? "r" : "l"]
        )});
        return cd;
    }

    public IReadOnlySet<ICardTraitEntry> GetInnateTraits(State state)
    {
        return upgrade == Upgrade.B? new HashSet<ICardTraitEntry>{ModEntry.Instance.KokoroApi.V2.Fleeting.Trait} : [];
    }
}