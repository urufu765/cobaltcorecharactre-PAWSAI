using System;
using System.Collections.Generic;
using System.Reflection;
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
                upgradesTo = [Upgrade.A, Upgrade.B]
            },
            Name = ModEntry.Instance.AnyLocalizations.Bind(["Pawsai", "card", rare.ToString(), MethodBase.GetCurrentMethod()!.DeclaringType!.Name, "name"]).Localize,
            Art = ModEntry.RegisterSprite(package, $"assets/card/pawsai/{rare}/{MethodBase.GetCurrentMethod()!.DeclaringType!.Name}.png").Sprite
        });
    }


    public override List<CardAction> GetActions(State s, Combat c)
    {
        int moveAmount = 0;
        if (ModEntry.Instance.settings.ProfileBased.Current.AccurateCalculations)
        {
            moveAmount = CalculatePreciselyHowMuchToMove(s, c, flipped);
        }
        else
        {
            moveAmount = GuessHowMuchToMove(s, c, flipped);
        }
        
        return upgrade switch
        {
            Upgrade.B =>
            [
                new AMove
                {
                    dir = moveAmount,
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
                    dir = moveAmount,
                    targetPlayer = true
                },
                new AEndTurn()
            ],
        };
    }

    private static int GuessHowMuchToMove(State s, Combat c, bool flip)
    {
        int moving;
        if (flip)  // move right
        {
            // check rightmost enemy with leftmost player
            int enemyRight = c.otherShip.x + c.otherShip.parts.Count;  // +1 just in case
            int playerLeft = s.ship.x;
            moving = Math.Min(enemyRight - playerLeft, 0);
        }
        else
        {
            int enemyLeft = c.otherShip.x;
            int playerRight = s.ship.x = s.ship.parts.Count;  // +1 just in case
            moving = Math.Max(enemyLeft - playerRight, 0);
        }
        return moving;
    }

    private static int CalculatePreciselyHowMuchToMove(State s, Combat c, bool flip)
    {
        int moving = 0;
        bool overlapping = s.ship.x < c.otherShip.x + c.otherShip.parts.Count && s.ship.x + s.ship.parts.Count > c.otherShip.x;
        while (overlapping)
        {
            moving += flip ? 1 : -1;

            bool noOverlappingParts = true;
            for (int i = s.ship.x + moving; i < s.ship.x + moving + s.ship.parts.Count; i++)
            {
                if (s.ship.GetPartAtWorldX(i) is Part sp && c.otherShip.GetPartAtWorldX(i) is Part cp && sp.type != PType.empty && cp.type != PType.empty)
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
        return moving;
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
            new List<string>() {
                ModEntry.Instance.Localizations.Localize(
                    ["Pawsai", "card", rare.ToString(), MethodBase.GetCurrentMethod()!.DeclaringType!.Name, flipped ? "r" : "l"]
        )});
        return cd;
    }

    public IReadOnlySet<ICardTraitEntry> GetInnateTraits(State state)
    {
        return upgrade == Upgrade.B? new HashSet<ICardTraitEntry>{ModEntry.Instance.KokoroApi.V2.Fleeting.Trait} : [];
    }
}