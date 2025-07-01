using System.Collections.Generic;
using System.Reflection;
using Nanoray.PluginManager;
using Nickel;

namespace Starhunters.Bruno.Cards;

/// <summary>
/// Bruno.
/// </summary>
public class BrunoExe : Card, IRegisterable
{
    public static void Register(IPluginPackage<IModManifest> package, IModHelper helper)
    {
        helper.Content.Cards.RegisterCard(new CardConfiguration
        {
            CardType = MethodBase.GetCurrentMethod()!.DeclaringType!,
            Meta = new CardMeta
            {
                rarity = Rarity.common,
                upgradesTo = [Upgrade.A, Upgrade.B]
            },
            Name = ModEntry.Instance.AnyLocalizations.Bind(["Bruno", "card", "Token", "EXE", "name"]).Localize,
            Art = ModEntry.RegisterSprite(package, "assets/card/bruno/EXE.png").Sprite
        });
    }


    public override List<CardAction> GetActions(State s, Combat c)
    {
        return upgrade switch
        {
            Upgrade.B => 
            [
                new ACardOffering
                {
                    amount = 3,
                    limitDeck = ModEntry.Instance.BrunoDeck.Deck,
                    makeAllCardsTemporary = true,
                    overrideUpgradeChances = false,
                    canSkip = false,
                    inCombat = true,
                    discount = -1,
                    dialogueSelector = ".summonBruno"
                }
            ],
            _ => 
            [
                new ACardOffering
                {
                    amount = 2,
                    limitDeck = ModEntry.Instance.BrunoDeck.Deck,
                    makeAllCardsTemporary = true,
                    overrideUpgradeChances = false,
                    canSkip = false,
                    inCombat = true,
                    discount = -1,
                    dialogueSelector = ".summonBruno"
                }
            ],
        };
    }


    public override CardData GetData(State state)
    {
        return upgrade switch
        {
            Upgrade.B => new CardData
            {
                cost = 1,
                exhaust = true,
                description = ColorlessLoc.GetDesc(state, 3, ModEntry.Instance.BrunoDeck.Deck),
                artTint = "764535"
            },
            Upgrade.A => new CardData
            {
                cost = 0,
                exhaust = true,
                description = ColorlessLoc.GetDesc(state, 2, ModEntry.Instance.BrunoDeck.Deck),
                artTint = "764535"
            },
            _ => new CardData
            {
                cost = 1,
                exhaust = true,
                description = ColorlessLoc.GetDesc(state, 2, ModEntry.Instance.BrunoDeck.Deck),
                artTint = "764535"
            }
        };
    }
}