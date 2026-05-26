using System;
using Microsoft.Extensions.Logging;

namespace Weth.Cards;

public enum TarmaucTheme
{
    Jumper,
    Base,  // Only obtainable through EXE or by having the Herald of Nihility relic.
    Strength,
    Infinity,  // Only reserved for final event
    Evanescent,
    Trance,  // Only available via trade event
    Magician,
    Lover
}

public abstract class TarmaucCard : Card
{
    /// <summary>
    /// Theming
    /// </summary>
    public virtual TarmaucTheme CardTheme { get; set; } = TarmaucTheme.Jumper;

    /// <summary>
    /// Burn status, for less typing
    /// </summary>
    public static Status Status_burn => ModEntry.Instance.BurnStatus.Status;

    /// <summary>
    /// Blister status, for less typing
    /// </summary>
    public static Status Status_blister => ModEntry.Instance.BlisterStatus.Status;

    /// <summary>
    /// Move Veto status, for less typing
    /// </summary>
    public static Status Status_moveVeto => ModEntry.Instance.VetoStatus.Status;

    /// <summary>
    /// Renders the rarity sprite, as well as extra bits if theme is present.
    /// </summary>
    /// <param name="g"></param>
    /// <param name="v"></param>
    public override void ExtraRender(G g, Vec v)
    {
        // double rarity_xOffset = -8.0;
        // double rarity_yOffset = -9.0;
        // CardMeta cm = GetMeta();
        // DeckDef dd = DB.decks[cm.deck];
        // // Rarity drawing portion
        // Draw.Sprite(
        //     cm.rarity switch
        //     {
        //         _ => StableSpr.cardShared_rare,
        //     },
        //     v.x + rarity_xOffset,
        //     v.y + rarity_yOffset,
        //     color: dd.color.gain(0.4)
        // );
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
            TarmaucTheme.Strength => null,
            TarmaucTheme.Infinity => null,
            TarmaucTheme.Evanescent => null,
            TarmaucTheme.Trance => null,
            TarmaucTheme.Magician => null,
            TarmaucTheme.Lover => null,
            _ => null
        };

        return data;
    }
}