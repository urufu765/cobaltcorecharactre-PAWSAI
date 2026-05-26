using System;
using Microsoft.Extensions.Logging;

namespace Weth.Cards;

public enum ZealType
{
    None,
    Leftie,
    Rightie,
    /// <summary>
    /// Make sure Ragebait type cards always retains... or develop a new trait that retains for a few turns ;)
    /// </summary>
    Ragebait
}

/// <summary>
/// Tarmauc Card that uses Zeal. Unused.
/// </summary>
public abstract class UNUSED_TarmaucCard : YellowCardTrash
{
    /// <summary>
    /// Theming
    /// </summary>
    public abstract TarmaucTheme CardTheme { get; set; }

    /// <summary>
    /// The current zeal amount of Tarmauc's card
    /// </summary>
    public int ZealAmount { get; set; }

    /// <summary>
    /// The base zeal of the card, override if different from default
    /// </summary>
    public virtual int BaseZealAmount { get; set; } = 0;

    /// <summary>
    /// Base zeal override that will override the card's default setting
    /// </summary>
    public int? BaseZealOverride { get; set; }

    /// <summary>
    /// The amount of zeal the player can have before incurring the wrath of blockable hurt. Default = 3 (L/R), 4 (Rage)
    /// </summary>
    public int? ZealSoftLimitOverride { get; set; }

    /// <summary>
    /// The max amount of zeal the player can ever achieve. Default = 1984
    /// </summary>
    public int? ZealHardLimitOverride { get; set; }

    /// <summary>
    /// Source of zeal increase, used for visual stuff
    /// </summary>
    public virtual ZealType ZealType => ZealTypeOverride ?? BaseZealType;

    /// <summary>
    /// The card's base Zeal type
    /// </summary>
    public virtual ZealType BaseZealType { get; set; }

    /// <summary>
    /// For overwriting the base zeal type
    /// </summary>
    public ZealType? ZealTypeOverride { get; set; }

    /// <summary>
    /// For detecting if the card's zeal type can be changed. Set true if it can't.
    /// </summary>
    public virtual bool ZealTypeLocked { get; set; }

    /// <summary>
    /// What types the card can be mutated into. Must contain itself at least, if the card can be mutated (TODO: Find a way to show which types the player can mutate the card into)
    /// </summary>
    //public virtual ZealType[]? AvailableTypes { get; set; }

    public const int ZEAL_HARD_LIMIT = 1984;
    public const int ZEAL_SOFT_DIRECTIONAL = 3;
    public const int ZEAL_SOFT_RAGE = 4;

    /// <summary>
    /// Renders the action right in the energy amount row, as well as the special card borders... actually let's let card borders be handled by game code if possible... Also investigate DB.deckBordersOver for fun border shenanigans
    /// </summary>
    /// <param name="g"></param>
    /// <param name="v"></param>
    public override void ExtraRender(G g, Vec v)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Maybe an artifact increases the zeal on draw? We have override base zeal for that, no?
    /// </summary>
    /// <param name="s"></param>
    /// <param name="c"></param>
    public override void OnDraw(State s, Combat c)
    {
        base.OnDraw(s, c);
    }

    /// <summary>
    /// Handles resetting the zeal
    /// </summary>
    /// <param name="s"></param>
    /// <param name="c"></param>
    public override void OnDiscard(State s, Combat c)
    {
        // Check for artifact that might retain some zeal?
        ZealAmount = BaseZealOverride ?? BaseZealAmount;
    }

    /// <summary>
    /// Do blockable hurt if applicable
    /// </summary>
    /// <param name="state"></param>
    /// <param name="c"></param>
    public override void AfterWasPlayed(State state, Combat c)
    {
        int softLimiter = GetSoftLimit(state);

        if (ZealAmount > softLimiter)
        {
            c.QueueImmediate(new AHurt
            {
                hurtAmount = ZealAmount - softLimiter,
                hurtShieldsFirst = true,
                targetPlayer = true
            });
        }
    }

    /// <summary>
    /// To show which cards will increase the zeal (might remove if it causes confusion)
    /// </summary>
    /// <param name="s"></param>
    /// <param name="c"></param>
    public override void HilightOtherCards(State s, Combat c)
    {
        if (ZealType is not (ZealType.Leftie or ZealType.Rightie)) return;

        int cardPos = c.hand.FindIndex(c => c.uuid == this.uuid);
        if (ZealType == ZealType.Leftie && cardPos > 0)
        {
            for (int i = 0; i < cardPos; i++)
            {
                Card? card = c.hand[i];
                if (card?.uuid is not null)
                {
                    c.hilightedCards.Add(card.uuid);
                }
                else
                {
                    ModEntry.Instance.Logger.LogWarning("HilightOtherCards Zealer leftie encountered a null card (or null uuid)!!");
                }
            }
        }
        if (ZealType == ZealType.Rightie && cardPos < c.hand.Count - 1)
        {
            for (int i = c.hand.Count - 1; i > cardPos; i--)
            {
                Card? card = c.hand[i];
                if (card?.uuid is not null)
                {
                    c.hilightedCards.Add(card.uuid);
                }
                else
                {
                    ModEntry.Instance.Logger.LogWarning("HilightOtherCards Zealer rightie encountered a null card (or null uuid)!!");
                }
            }
        }
    }

    /// <summary>
    /// Get the soft limit value
    /// </summary>
    public int GetSoftLimit(State s)
    {
        // Maybe have an artifact increase the soft limit by 1?
        return ZealSoftLimitOverride ?? ZealType switch
        {
            ZealType.Leftie or ZealType.Rightie => ZEAL_SOFT_DIRECTIONAL,
            ZealType.Ragebait => ZEAL_SOFT_RAGE,
            _ => int.MaxValue
        };
    }

    /// <summary>
    /// Increases zeal amount, while staying below the hard limit
    /// </summary>
    /// <param name="increase">Amount to increase by (maybe artifact mod potential)</param>
    /// <returns>New incremented ZealAmount</returns>
    public int GetIncrementedValue(State s, int increase = 1)
    {
        return Math.Min(ZealAmount + increase, ZealHardLimitOverride ?? ZEAL_HARD_LIMIT);
    }

    public int GetAmount(int noneAmount = 0)
    {
        return ZealType == ZealType.None ? noneAmount : ZealAmount;
    }

    /// <summary>
    /// Custom hook that happens before other card is played, so that a shuffle card action doesn't affect the results. Around where the artifact hook would go
    /// </summary>
    /// <param name="playedCardPos">Hand position of the card that is about to play</param>
    public virtual void WhenOtherCardPlayedWhileZealCardIsInHand(State s, Combat c, int playedCardPos)
    {
        // Reduce cost of operation, if even by a little
        if (ZealType is not (ZealType.Leftie or ZealType.Rightie)) return;

        int cardPos = c.hand.FindIndex(c => c.uuid == this.uuid);
        if (cardPos == -1)
        {
            ModEntry.Instance.Logger.LogWarning("{CardName} Tarmauc Card doesn't exist in hand?!", this.GetFullDisplayName());
            return;
        }

        if (
                playedCardPos < cardPos && ZealType == ZealType.Leftie ||
                playedCardPos > cardPos && ZealType == ZealType.Rightie
            )
        {
            ZealAmount = GetIncrementedValue(s);
            // Add sfx... and vfx?
            flopAnim += flopAnim > 0.2 ? 0.3 : 0.7;
        }
    }

    /// <summary>
    /// Custom hook that happens when player ship is hit (right before AAttack line 162 Part partAtWorldX...)
    /// </summary>
    /// <param name="s"></param>
    /// <param name="c"></param>
    public virtual void WhenAShipIsHit(State s, Combat c, bool isBeam = false, bool playerHit = false)
    {
        if (isBeam) return;
        if (playerHit && ZealType == ZealType.Ragebait)
        {
            ZealAmount = GetIncrementedValue(s);
            // Add sfx?
            shakeNoAnim += shakeNoAnim > 0.3 ? 0.3 : 0.8;
        }
    }

    /// <summary>
    /// Basically GetData, except it's separated like this such that I don't have to assign the overlay sprites for EVERY card. TODO: do the same with Weth lol
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    public abstract CardData GetPreData(State state);

    /// <summary>
    /// DO NOT OVERRIDE THIS FOR TARMAUC CARDS, use GetPreData instead.
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    public override CardData GetData(State state)
    {
        CardData data = GetPreData(state);

        data.artOverlay ??= CardTheme switch
        {
            // TarmaucCardType.Leftie => null,
            // TarmaucCardType.Rightie => null,
            // TarmaucCardType.Ragebait => null,
            _ => null
        };

        return data;
    }
}