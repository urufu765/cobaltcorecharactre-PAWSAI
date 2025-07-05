using System.Collections.Generic;
using System.Reflection;
using Nanoray.PluginManager;
using Nickel;

namespace Starhunters.Kodijen.Cards;

/// <summary>
/// Kodijen.
/// </summary>
public class KodijenExe : Card, IRegisterable
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
            Name = ModEntry.Instance.AnyLocalizations.Bind(["Kodijen", "card", "token", "EXE", "name"]).Localize,
            Art = ModEntry.RegisterSprite(package, "assets/card/kodijen/EXE.png").Sprite
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
                    limitDeck = ModEntry.Instance.KodijenDeck.Deck,
                    makeAllCardsTemporary = true,
                    overrideUpgradeChances = false,
                    canSkip = false,
                    inCombat = true,
                    discount = -1,
                    dialogueSelector = ".summonKodijen"
                }
            ],
            _ => 
            [
                new ACardOffering
                {
                    amount = 2,
                    limitDeck = ModEntry.Instance.KodijenDeck.Deck,
                    makeAllCardsTemporary = true,
                    overrideUpgradeChances = false,
                    canSkip = false,
                    inCombat = true,
                    discount = -1,
                    dialogueSelector = ".summonKodijen"
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
                description = ColorlessLoc.GetDesc(state, 3, ModEntry.Instance.KodijenDeck.Deck),
                artTint = "e4b83e"
            },
            Upgrade.A => new CardData
            {
                cost = 0,
                exhaust = true,
                description = ColorlessLoc.GetDesc(state, 2, ModEntry.Instance.KodijenDeck.Deck),
                artTint = "e4b83e"
            },
            _ => new CardData
            {
                cost = 1,
                exhaust = true,
                description = ColorlessLoc.GetDesc(state, 2, ModEntry.Instance.KodijenDeck.Deck),
                artTint = "e4b83e"
            }
        };
    }
}