using HarmonyLib;
using Nickel;
using System;
using System.Collections.Generic;
using System.Linq;
using JollyJolly.Artifacts;
//using JollyJolly.Cards;
using JollyJolly.External;
using JollyJolly.Conversation;

namespace JollyJolly;

internal partial class ModEntry : SimpleMod
{
    internal static ModEntry Instance { get; private set; } = null!;
    internal static IPlayableCharacterEntryV2 WethTheSnep { get; private set; } = null!;
    internal string UniqueName { get; private set; }
    internal Harmony Harmony;
    internal IKokoroApi KokoroApi;
    internal IDeckEntry WethDeck;
    internal IDeckEntry GoodieDeck;
    public bool modDialogueInited;
    private int _loadFrameBuffer = 3;
    public bool WethFrameLoadAllowed
    {
        get => _loadFrameBuffer-- > 0;
    }
    internal IStatusEntry PulseStatus { get; private set; } = null!;
    internal IStatusEntry UnknownStatus { get; private set; } = null!;
    internal IModSoundEntry JauntSlapSound { get; private set; }
    internal IModSoundEntry SodaOpening { get; private set; }
    internal IModSoundEntry SodaOpened { get; private set; }
    internal IModSoundEntry HitHullHit { get; private set; }
    // internal IModSoundEntry MidiTestJourneyV { get; private set; }
    // internal IModSoundEntry MidiTestIncompetentB { get; private set; }
    public Spr PulseQuestionMark { get; private set; }
    // internal ICardTraitEntry AutoSU { get; private set; } = null!;
    // internal Spr AutoSUSpr { get; private set; }
    //internal ICardTraitEntry AutoE { get; private set; } = null!;


    internal ILocalizationProvider<IReadOnlyList<string>> AnyLocalizations { get; }
    internal ILocaleBoundNonNullLocalizationProvider<IReadOnlyList<string>> Localizations { get; }
    internal IMoreDifficultiesApi? MoreDifficultiesApi { get; private set; } = null!;
    internal IDuoArtifactsApi? DuoArtifactsApi { get; private set; } = null!;
    public LocalDB localDB { get; set; } = null!;
    
    /*
     * The following lists contain references to all types that will be registered to the game.
     * All cards and artifacts must be registered before they may be used in the game.
     * In theory only one collection could be used, containing all registerable types, but it is seperated this way for ease of organization.
     */
    private readonly static List<Type> WethCommonCardTypes = [

    ];
    private readonly static List<Type> WethUncommonCardTypes = [

    ];
    private readonly static List<Type> WethRareCardTypes = [

    ];
    private readonly static List<Type> WethSpecialCardTypes = [

    ];
    private readonly static IEnumerable<Type> WethCardTypes =
        WethCommonCardTypes
            .Concat(WethUncommonCardTypes)
            .Concat(WethRareCardTypes)
            .Concat(WethSpecialCardTypes);

    private readonly static List<Type> WethCommonArtifacts = [

    ];
    private readonly static List<Type> WethBossArtifacts = [

    ];
    private readonly static List<Type> WethEventArtifacts = [

    ];
    private readonly static List<Type> WethSpecialArtifacts = [

    ];
    private readonly static List<Type> WethDuoArtifacts = [
        // typeof(),  // CAT
        // typeof(),  // Peri
        // typeof(),  // Isaac
        // typeof(),  // Books
        // typeof(),  // Drake
        // typeof(),  // Dizzy
        // typeof(),  // Riggs
        // typeof(),  // Max
    ];
    private readonly static IEnumerable<Type> WethArtifactTypes =
        WethCommonArtifacts
            .Concat(WethBossArtifacts)
            .Concat(WethEventArtifacts)
            .Concat(WethSpecialArtifacts)
            .Concat(WethDuoArtifacts);

    private readonly static List<Type> WethDialogues = [
        typeof(StoryDialogue),
        typeof(EventDialogue),
        typeof(CombatDialogue),
        typeof(ArtifactDialogue),
        typeof(CardDialogue),
        typeof(MemoryDialogue)
    ];
    private readonly static IEnumerable<Type> AllRegisterableTypes =
        WethCardTypes
            .Concat(WethDialogues);

    private static List<string> Weth1Anims = [
        "gameover",
        "mini",
        "placeholder",
    ];
    private static List<string> Weth3Anims = [
    ];
    private static List<string> Weth4Anims = [
    ];
    private static List<string> Weth5Anims = [
        "neutral",
        "squint",
    ];
    private static List<string> Weth6Anims = [
    ];
    public readonly static IEnumerable<string> WethAnims =
        Weth1Anims
            .Concat(Weth3Anims)
            .Concat(Weth4Anims)
            .Concat(Weth5Anims)
            .Concat(Weth6Anims);
}