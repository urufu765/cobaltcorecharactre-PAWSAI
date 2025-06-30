using System;
using System.Collections.Generic;
using System.Reflection;
using Nanoray.PluginManager;
using Nickel;

namespace Starhunters.Pawsai.Cards;

public enum SmartAlignmentMode
{
    none,
    nonArmored,
    weak,
    brittle
}

/// <summary>
/// Pawsai.
/// </summary>
public class SmartAlignment : Card, IRegisterable
{
    public SmartAlignmentMode availableMode;
    public bool museumMode = false;
    public double clock = 0;

    private static Rarity rare = Rarity.rare;
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
        availableMode = FindClosestDestination(s, c, out int moveAmount);


        return upgrade switch
        {
            _ =>
            [
                new AMove
                {
                    dir = moveAmount,
                    targetPlayer = true
                }
            ],
        };
    }

    /// <summary>
    /// Finds the closest destination of something... TODO: Optimize
    /// </summary>
    /// <param name="s"></param>
    /// <param name="c"></param>
    /// <param name="movement"></param>
    /// <returns></returns>
    private SmartAlignmentMode FindClosestDestination(State s, Combat c, out int movement)
    {
        // Check for cannon then get the worldX
        int cannonX = s.ship.x;
        for (int i = 0; i < s.ship.parts.Count; i++)
        {
            if (s.ship.parts[i].type == PType.cannon)
            {
                cannonX = s.ship.x + i;
                if (!flipped) break; // First cannon is left cannon
            }
        }

        SmartAlignmentMode sam = SmartAlignmentMode.none;
        int brittleX = 1000;
        int weakX = 1000;
        int normalX = 1000;
        // check the location of nearest brittle (ignore hidden)
        // check the other locations too, then fall back if not found
        for (int j = 0; j < c.otherShip.parts.Count; j++)
        {
            if (c.otherShip.parts[j].GetDamageModifier() == PDamMod.brittle && !c.otherShip.parts[j].brittleIsHidden)
            {
                if (sam < SmartAlignmentMode.brittle)
                {
                    sam = SmartAlignmentMode.brittle;
                }
                if (Math.Abs(c.otherShip.x + j - cannonX) < Math.Abs(brittleX))
                {
                    brittleX = c.otherShip.x + j - cannonX;
                }
                else if (Math.Abs(c.otherShip.x + j - cannonX) == Math.Abs(brittleX))
                {
                    if (flipped)  // Right cannon, Left bias
                    {
                        if (c.otherShip.x + j - cannonX < brittleX)
                        {
                            brittleX = c.otherShip.x + j - cannonX;
                        }
                    }
                    else  // Left cannon, right bias
                    {
                        if (c.otherShip.x + j - cannonX > brittleX)
                        {
                            brittleX = c.otherShip.x + j - cannonX;
                        }
                    }
                }
            }
            else if (c.otherShip.parts[j].GetDamageModifier() == PDamMod.weak && sam <= SmartAlignmentMode.weak)
            {
                if (sam < SmartAlignmentMode.weak)
                {
                    sam = SmartAlignmentMode.weak;
                }
                if (Math.Abs(c.otherShip.x + j - cannonX) < Math.Abs(weakX))
                {
                    weakX = c.otherShip.x + j - cannonX;
                }
                else if (Math.Abs(c.otherShip.x + j - cannonX) == Math.Abs(weakX))
                {
                    if (flipped)  // Right cannon, Left bias
                    {
                        if (c.otherShip.x + j - cannonX < weakX)
                        {
                            weakX = c.otherShip.x + j - cannonX;
                        }
                    }
                    else  // Left cannon, right bias
                    {
                        if (c.otherShip.x + j - cannonX > weakX)
                        {
                            weakX = c.otherShip.x + j - cannonX;
                        }
                    }
                }
            }
            else if (c.otherShip.parts[j].GetDamageModifier() == PDamMod.none && sam <= SmartAlignmentMode.nonArmored)
            {
                if (sam < SmartAlignmentMode.nonArmored)
                {
                    sam = SmartAlignmentMode.nonArmored;
                }
                if (Math.Abs(c.otherShip.x + j - cannonX) < Math.Abs(normalX))
                {
                    normalX = c.otherShip.x + j - cannonX;
                }
                else if (Math.Abs(c.otherShip.x + j - cannonX) == Math.Abs(normalX))
                {
                    if (flipped)  // Right cannon, Left bias
                    {
                        if (c.otherShip.x + j - cannonX < normalX)
                        {
                            normalX = c.otherShip.x + j - cannonX;
                        }
                    }
                    else  // Left cannon, right bias
                    {
                        if (c.otherShip.x + j - cannonX > normalX)
                        {
                            normalX = c.otherShip.x + j - cannonX;
                        }
                    }
                }
            }
        }

        movement = sam switch
        {
            SmartAlignmentMode.brittle => brittleX,
            SmartAlignmentMode.weak => weakX,
            SmartAlignmentMode.nonArmored => normalX,
            _ => 0
        };
        return sam;
    }


    public override CardData GetData(State state)
    {
        if (museumMode)
        {
            availableMode = (Math.Round(clock) % 3) switch
            {
                0 => SmartAlignmentMode.nonArmored,
                1 => SmartAlignmentMode.weak,
                2 => SmartAlignmentMode.brittle,
                _ => SmartAlignmentMode.none
            };
        }
        CardData cd = upgrade switch
        {
            Upgrade.B => new CardData
            {
                cost = 0
            },
            Upgrade.A => new CardData
            {
                cost = 1,
                flippable = true,
                retain = true
            },
            _ => new CardData
            {
                cost = 1,
                flippable = true
            }
        };

        cd.description = ModEntry.Instance.Localizations.Localize(["Pawsai", "card", rare.ToString(), MethodBase.GetCurrentMethod()!.DeclaringType!.Name, "desc"], new
        {
            direction = ModEntry.Instance.Localizations.Localize(["Pawsai", "card", rare.ToString(), MethodBase.GetCurrentMethod()!.DeclaringType!.Name, flipped ? "l" : "r"]),
            part = ModEntry.Instance.Localizations.Localize(["Pawsai", "card", rare.ToString(), MethodBase.GetCurrentMethod()!.DeclaringType!.Name,
                availableMode switch
                {
                    SmartAlignmentMode.nonArmored => "n",
                    SmartAlignmentMode.weak => "w",
                    SmartAlignmentMode.brittle => "b",
                    _ => "f"
                }
            ]),
        });
        return cd;
    }

    public override void ExtraRender(G g, Vec v)
    {
        if (g.state?.route is Combat)
        {
            museumMode = false;
        }
        else if (g.state?.route is not Combat)
        {
            museumMode = true;
        }
        clock += g.dt / 2;
    }
}