using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Nanoray.PluginManager;
using Nickel;

namespace Starhunters.Bruno.Cards;

/// <summary>
/// Bruno.
/// </summary>
public class DeepClean : Card, IRegisterable
{
    private static Rarity rare = Rarity.rare;
    public static void Register(IPluginPackage<IModManifest> package, IModHelper helper)
    {
        helper.Content.Cards.RegisterCard(new CardConfiguration
        {
            CardType = MethodBase.GetCurrentMethod()!.DeclaringType!,
            Meta = new CardMeta
            {
                rarity = rare,
                upgradesTo = [Upgrade.A],
                deck = ModEntry.Instance.BrunoDeck.Deck
            },
            Name = ModEntry.Instance.AnyLocalizations.Bind(["Bruno", "card", rare.ToString(), MethodBase.GetCurrentMethod()!.DeclaringType!.Name, "name"]).Localize,
            Art = ModEntry.RegisterSprite(package, $"assets/card/bruno/{rare}/{MethodBase.GetCurrentMethod()!.DeclaringType!.Name}.png").Sprite
        });
    }


    public override List<CardAction> GetActions(State s, Combat c)
    {
        int x = c.hand.Count - 1;
        int y = c.energy;
        List<CardAction> actions = [
            new AStatus
            {
                status = ModEntry.Instance.Status_Mitigate.Status,
                statusAmount = x,
                targetPlayer = true
            },
            new AStatus
            {
                status = ModEntry.Instance.Status_Hyperdrive.Status,
                statusAmount = y,
                targetPlayer = true
            }
        ];
        List<int> uuids = [.. c.hand.Where(card => card.uuid != this.uuid).Select(card => card.uuid)];
        actions.AddRange(
            upgrade == Upgrade.A?
                uuids.Select(u => new ADestroyCard { uuid = u }) :
                [new AExhaustEntireHand()]
        );
        return actions;
    }


    public override CardData GetData(State state)
    {
        CardData cd = upgrade switch
        {
            Upgrade.A => new CardData
            {
                cost = 0,
                singleUse = true
            },
            _ => new CardData
            {
                cost = 0,
                exhaust = true
            }
        };

        cd.description = ModEntry.Instance.Localizations.Localize(["Bruno", "card", rare.ToString(), MethodBase.GetCurrentMethod()!.DeclaringType!.Name, upgrade switch {
                Upgrade.A => "descA",
                _ => "desc"
            }]);
        return cd;
    }
}