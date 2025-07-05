using System.Collections.Generic;
using System.Reflection;
using Nanoray.PluginManager;
using Nickel;

namespace Starhunters.Kodijen.Cards;

/// <summary>
/// Kodijen.
/// </summary>
public class ReloadUseOnly : Card, IRegisterable
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
                deck = ModEntry.Instance.KodijenDeck.Deck
            },
            Name = ModEntry.Instance.AnyLocalizations.Bind(["Kodijen", "card", rare.ToString(), MethodBase.GetCurrentMethod()!.DeclaringType!.Name, "name"]).Localize,
            Art = ModEntry.RegisterSprite(package, $"assets/card/kodijen/{rare}/{MethodBase.GetCurrentMethod()!.DeclaringType!.Name}.png").Sprite
        });
    }


    public override List<CardAction> GetActions(State s, Combat c)
    {
        return upgrade switch
        {
            Upgrade.B =>
            [
                new ASpawn
                {
                    thing = new Missile{yAnimation = 0, targetPlayer = true, missileType = MissileType.seeker}
                }
            ],
            Upgrade.A =>
            [
                new ASpawn
                {
                    thing = new Missile{yAnimation = 0, targetPlayer = true, missileType = MissileType.heavy}
                }
            ],
            _ =>
            [
                new ASpawn
                {
                    thing = new Missile{yAnimation = 0, targetPlayer = true, missileType = MissileType.normal}
                }
            ],
        };
    }


    public override CardData GetData(State state)
    {
        return upgrade switch
        {
            _ => new CardData
            {
                cost = 1,
            }
        };
    }
}