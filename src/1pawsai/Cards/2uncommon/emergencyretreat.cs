using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Logging;
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
                deck = ModEntry.Instance.PawsaiDeck.Deck,
                upgradesTo = [Upgrade.A, Upgrade.B]
            },
            Name = ModEntry.Instance.AnyLocalizations.Bind(["Pawsai", "card", rare.ToString(), MethodBase.GetCurrentMethod()!.DeclaringType!.Name, "name"]).Localize,
            Art = ModEntry.RegisterSprite(package, $"assets/card/pawsai/{rare}/{MethodBase.GetCurrentMethod()!.DeclaringType!.Name}.png").Sprite
        });
    }


    public override List<CardAction> GetActions(State s, Combat c)
    {
        int toMove;
        if (ModEntry.Instance.settings.ProfileBased.Current.Pawsai_BetterEmergencyRetreat)
        {
            toMove = CalculatePreciselyHowMuchToMove(s, c);
        }
        else
        {
            toMove = GuessHowMuchToMove(s, c);
        }
        
        return upgrade switch
        {
            Upgrade.B =>
            [
                new AMove
                {
                    dir = toMove,
                    targetPlayer = true
                },
                new AStatus
                {
                    status = Status.evade,
                    statusAmount = 0,
                    mode = AStatusMode.Set,
                    targetPlayer = true
                },
                new AEndTurn()
            ],
            _ =>
            [
                new AMove
                {
                    dir = toMove,
                    targetPlayer = true
                },
                new AEndTurn()
            ],
        };
    }

    private int GuessHowMuchToMove(State s, Combat c)
    {
        if (s.ship.x >= c.otherShip.x + c.otherShip.parts.Count || s.ship.x + s.ship.parts.Count <= c.otherShip.x)
        {
            return 0;
        }
        
        int moving;
        if (!flipped)  // move right
        {
            // check rightmost enemy with leftmost player
            int enemyRight = c.otherShip.x + c.otherShip.parts.Count;  // +1 just in case
            int playerLeft = s.ship.x;
            moving = enemyRight - playerLeft;
        }
        else
        {
            int enemyLeft = c.otherShip.x;
            int playerRight = s.ship.x + s.ship.parts.Count;  // +1 just in case
            moving = enemyLeft - playerRight;
        }
        return Math.Abs(moving);
    }

    public int CalculatePreciselyHowMuchToMove(State s, Combat c)
    {
        int moving = 0;
        bool overlapping = s.ship.x < c.otherShip.x + c.otherShip.parts.Count && s.ship.x + s.ship.parts.Count > c.otherShip.x;
        while (overlapping)
        {
            moving += flipped ? -1 : 1;

            bool noOverlappingParts = true;
            for (int i = 0; i < s.ship.parts.Count; i++)
            {
                if (s.ship.parts[i] is Part sp && c.otherShip.GetPartAtWorldX(i + s.ship.x + moving) is Part cp && sp.type != PType.empty && cp.type != PType.empty)
                {
                    noOverlappingParts = false;
                    break;
                }
            }

            if (noOverlappingParts)
            {
                overlapping = false;
            }

            if (s.ship.x >= c.otherShip.x + c.otherShip.parts.Count || s.ship.x + s.ship.parts.Count <= c.otherShip.x)
            {
                overlapping = false;
            }
        }
        return Math.Abs(moving);
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
            new {
                direction = ModEntry.Instance.Localizations.Localize(
                    ["Pawsai", "card", rare.ToString(), MethodBase.GetCurrentMethod()!.DeclaringType!.Name, flipped ? "l" : "r"]
        )});
        return cd;
    }

    public IReadOnlySet<ICardTraitEntry> GetInnateTraits(State state)
    {
        return upgrade == Upgrade.B? new HashSet<ICardTraitEntry>{ModEntry.Instance.KokoroApi.V2.Fleeting.Trait} : [];
    }
}