using System.Collections.Generic;
using System.Reflection;
using Nanoray.PluginManager;
using Nickel;
using Starhunters.Bruno.Actions;

namespace Starhunters.Bruno.Cards;

/// <summary>
/// Bruno.
/// </summary>
public class TouchyTrigger : Card, IRegisterable, IHasCustomCardTraits
{
    private static Rarity rare = Rarity.common;
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
                new AHeavyAttack
                {
                    damage = GetDmg(s, 1),
                    baseDamage = 1
                },
                new AStatus
                {
                    status = Status.tempShield,
                    statusAmount = 2,
                    targetPlayer = true
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
                new AHeavyAttack
                {
                    damage = GetDmg(s, 1),
                    baseDamage = 1
                },
                new AStatus
                {
                    status = Status.tempShield,
                    statusAmount = 4,
                    targetPlayer = true
                },
            ],
            _ =>
            [
                new AHeavyAttack
                {
                    damage = GetDmg(s, 1),
                    baseDamage = 1
                },
                new AStatus
                {
                    status = Status.tempShield,
                    statusAmount = 2,
                    targetPlayer = true
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
                attack = GetDmg(state, 1),
                temp = upgrade == Upgrade.A ? 4 : 2
            });
        return cd;
    }

    public IReadOnlySet<ICardTraitEntry> GetInnateTraits(State state)
    {
        return new HashSet<ICardTraitEntry>{ModEntry.Instance.KokoroApi.V2.Fleeting.Trait};
    }
}