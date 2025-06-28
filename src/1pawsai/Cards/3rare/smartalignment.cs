using System.Collections.Generic;
using System.Reflection;
using Nanoray.PluginManager;
using Nickel;

namespace Starhunters.Pawsai.Cards;

public enum SmartAlignmentMode
{
    none,
    nonArmored,
    weak,
    brittle
}

/// <summary>
/// Pawsai.
/// </summary>
public class SmartAlignment : Card, IRegisterable, IHasCustomCardTraits
{
    public SmartAlignmentMode availableMode;
    private static Rarity rare = Rarity.rare;
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
        // TODO: Line up shit
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
            },
            Upgrade.A => new CardData
            {
                cost = 1,
                flippable = true,
                exhaust = true,
            },
            _ => new CardData
            {
                cost = 1,
                exhaust = true
            }
        };

        cd.description = ModEntry.Instance.Localizations.Localize(["Pawsai", "card", rare.ToString(), MethodBase.GetCurrentMethod()!.DeclaringType!.Name, "desc"], new List<string>()
        {
            ModEntry.Instance.Localizations.Localize(["Pawsai", "card", rare.ToString(), MethodBase.GetCurrentMethod()!.DeclaringType!.Name, flipped ? "l" : "r"]),
            ModEntry.Instance.Localizations.Localize(["Pawsai", "card", rare.ToString(), MethodBase.GetCurrentMethod()!.DeclaringType!.Name,
                availableMode switch
                {
                    SmartAlignmentMode.nonArmored => "n",
                    SmartAlignmentMode.weak => "w",
                    SmartAlignmentMode.brittle => "b",
                    _ => "f"
                }
            ]),
        });
        return cd;
    }

    public IReadOnlySet<ICardTraitEntry> GetInnateTraits(State state)
    {
        return upgrade == Upgrade.B? new HashSet<ICardTraitEntry>{ModEntry.Instance.KokoroApi.V2.Fleeting.Trait} : [];
    }
}