using System.Collections.Generic;
using System.Reflection;
using Nanoray.PluginManager;
using Nickel;

namespace Starhunters.Bruno.Cards;

/// <summary>
/// Bruno.
/// </summary>
public class MomentaryBoost : Card, IRegisterable, IHasCustomCardTraits
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
                upgradesTo = [Upgrade.A, Upgrade.B],
                deck = ModEntry.Instance.BrunoDeck.Deck
            },
            Name = ModEntry.Instance.AnyLocalizations.Bind(["Bruno", "card", rare.ToString(), MethodBase.GetCurrentMethod()!.DeclaringType!.Name, "name"]).Localize,
            Art = ModEntry.RegisterSprite(package, $"assets/card/bruno/{rare}/{MethodBase.GetCurrentMethod()!.DeclaringType!.Name}.png").Sprite
        });
    }


    public override void OnDraw(State s, Combat c)
    {
        c.QueueImmediate(upgrade switch
        {
            Upgrade.B =>
            [
                new AStatus
                {
                    status = ModEntry.Instance.Status_Hyperdrive.Status,
                    statusAmount = 1,
                    targetPlayer = true
                },
                new AEnergy
                {
                    changeAmount = 1
                },
                new AStatus
                {
                    status = ModEntry.Instance.Status_Mitigate.Status,
                    statusAmount = 1,
                    targetPlayer = true
                }
            ],
            Upgrade.A =>
            [
                new AStatus
                {
                    status = ModEntry.Instance.Status_Hyperdrive.Status,
                    statusAmount = 2,
                    targetPlayer = true
                },
                new AEnergy
                {
                    changeAmount = 1
                },
            ],
            _ =>
            [
                new AStatus
                {
                    status = ModEntry.Instance.Status_Hyperdrive.Status,
                    statusAmount = 1,
                    targetPlayer = true
                },
                new AEnergy
                {
                    changeAmount = 1
                },
            ],
        });
    }


    public override CardData GetData(State state)
    {
        CardData cd = upgrade switch
        {
            _ => new CardData
            {
                cost = 0,
                unplayable = true
            }
        };

        cd.description = ModEntry.Instance.Localizations.Localize(["Bruno", "card", rare.ToString(), MethodBase.GetCurrentMethod()!.DeclaringType!.Name, upgrade switch {
                Upgrade.B => "descB",
                _ => "desc"
            }], new
            {
                hyper = upgrade == Upgrade.A ? 2 : 1
            });
        return cd;
    }


    public IReadOnlySet<ICardTraitEntry> GetInnateTraits(State state)
    {
        return new HashSet<ICardTraitEntry> { ModEntry.Instance.KokoroApi.V2.Fleeting.Trait };
    }
}