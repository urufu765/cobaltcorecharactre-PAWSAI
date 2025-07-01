using System.Collections.Generic;
using System.Reflection;
using Nanoray.PluginManager;
using Nickel;

namespace Starhunters.Pawsai.Cards;

/// <summary>
/// Pawsai.
/// </summary>
public class PawsaiExe : Card, IRegisterable
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
            Name = ModEntry.Instance.AnyLocalizations.Bind(["Pawsai", "card", "Token", "EXE", "name"]).Localize,
            Art = ModEntry.RegisterSprite(package, "assets/card/pawsai/EXE.png").Sprite
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
                    limitDeck = ModEntry.Instance.PawsaiDeck.Deck,
                    makeAllCardsTemporary = true,
                    overrideUpgradeChances = false,
                    canSkip = false,
                    inCombat = true,
                    discount = -1,
                    dialogueSelector = ".summonPAWSAI"
                }
            ],
            _ => 
            [
                new ACardOffering
                {
                    amount = 2,
                    limitDeck = ModEntry.Instance.PawsaiDeck.Deck,
                    makeAllCardsTemporary = true,
                    overrideUpgradeChances = false,
                    canSkip = false,
                    inCombat = true,
                    discount = -1,
                    dialogueSelector = ".summonPAWSAI"
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
                description = ColorlessLoc.GetDesc(state, 3, ModEntry.Instance.PawsaiDeck.Deck),
                artTint = "98a9ba"
            },
            Upgrade.A => new CardData
            {
                cost = 0,
                exhaust = true,
                description = ColorlessLoc.GetDesc(state, 2, ModEntry.Instance.PawsaiDeck.Deck),
                artTint = "98a9ba"
            },
            _ => new CardData
            {
                cost = 1,
                exhaust = true,
                description = ColorlessLoc.GetDesc(state, 2, ModEntry.Instance.PawsaiDeck.Deck),
                artTint = "98a9ba"
            }
        };
    }
}