using System.Collections.Generic;
using System.Reflection;
using Nanoray.PluginManager;
using Nickel;

namespace Starhunters.Kodijen.Cards;

/// <summary>
/// Kodijen.
/// </summary>
public class AutoSignal : Card, IRegisterable
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
                upgradesTo = [Upgrade.A],
                deck = ModEntry.Instance.KodijenDeck.Deck
            },
            Name = ModEntry.Instance.AnyLocalizations.Bind(["Kodijen", "card", rare.ToString(), MethodBase.GetCurrentMethod()!.DeclaringType!.Name, "name"]).Localize,
            Art = ModEntry.RegisterSprite(package, $"assets/card/kodijen/{rare}/{MethodBase.GetCurrentMethod()!.DeclaringType!.Name}.png").Sprite
        });
    }


    public override void OnDraw(State s, Combat c)
    {
        c.QueueImmediate(new ADroneTurn
        {
            ignoreEnemyTurn = true
        });
    }

    public override CardData GetData(State state)
    {
        CardData cd = upgrade switch
        {
            Upgrade.A => new CardData
            {
                cost = 1,
                recycle = true
            },
            _ => new CardData
            {
                cost = 0,
                unplayable = true
            }
        };

        cd.description = ModEntry.Instance.Localizations.Localize(["Kodijen", "card", rare.ToString(), MethodBase.GetCurrentMethod()!.DeclaringType!.Name, "desc"]);
        return cd;
    }
}