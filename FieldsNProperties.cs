using HarmonyLib;
using Nickel;
using System;
using System.Collections.Generic;
using System.Linq;
using Starhunters.Artifacts;
//using Starhunters.Cards;
using Starhunters.External;
using Starhunters.Conversation;
using Starhunters.Pawsai.Cards;
using Nanoray.PluginManager;
using Nanoray.EnumByNameSourceGenerator;
using Starhunters.Pawsai.Artifacts;
using Starhunters.Bruno.Cards;

namespace Starhunters;

internal partial class ModEntry : SimpleMod
{
    internal static ModEntry Instance { get; private set; } = null!;
    internal static IPlayableCharacterEntryV2 Pawsai { get; private set; } = null!;  // Defense
    internal static IPlayableCharacterEntryV2 Bruno { get; private set; } = null!;  // Offense
    // internal static IPlayableCharacterEntryV2 Kodijen { get; private set; } = null!;  // Mid-row
    // internal static IPlayableCharacterEntryV2 Parmesan { get; private set; } = null!;  // Movement? (Teleportation)
    // internal static IPlayableCharacterEntryV2 Varrigradona { get; private set; } = null!;  // ???
    internal string UniqueName { get; private set; }
    internal Harmony Harmony;
    internal IKokoroApi KokoroApi;
    internal ILocalizationProvider<IReadOnlyList<string>> AnyLocalizations { get; }
    internal ILocaleBoundNonNullLocalizationProvider<IReadOnlyList<string>> Localizations { get; }
    internal IMoreDifficultiesApi? MoreDifficultiesApi { get; private set; }
    internal IDuoArtifactsApi? DuoArtifactsApi { get; private set; }
    internal Settings settings;
    private IWritableFileInfo SettingsFile => Helper.Storage.GetMainStorageFile("json");

    public LocalDB localDB { get; set; } = null!;
    // internal IModSoundEntry MidiTestJourneyV { get; private set; }
    // internal IModSoundEntry MidiTestIncompetentB { get; private set; }
    //public Spr PulseQuestionMark { get; private set; }
    // internal ICardTraitEntry AutoSU { get; private set; } = null!;
    // internal Spr AutoSUSpr { get; private set; }
    //internal ICardTraitEntry AutoE { get; private set; } = null!;

    #region PAWSAI stuff
    internal IDeckEntry PawsaiDeck;
    internal IStatusEntry Status_Repulsion { get; private set; }
    internal IStatusEntry Status_Repetition { get; private set; }
    internal IStatusEntry Status_Regeneration { get; private set; }

    [SpriteLoading("icon", "pawsai")]
    public Spr Action_MirrorMove_Random { get; private set; }
    [SpriteLoading("icon", "pawsai")]
    public Spr Action_MirrorMove_RandomLeft { get; private set; }
    [SpriteLoading("icon", "pawsai")]
    public Spr Action_MirrorMove_Zero { get; private set; }
    [SpriteLoading("icon", "pawsai")]
    public Spr Action_MirrorMove_Left { get; private set; }
    [SpriteLoading("icon", "pawsai")]
    public Spr Action_MirrorMove_Right { get; private set; }
    [SpriteLoading("icon", "pawsai")]
    public Spr Action_PivotIn_Zero { get; private set; }
    [SpriteLoading("icon", "pawsai")]
    public Spr Action_PivotOut_Zero { get; private set; }
    [SpriteLoading("icon", "pawsai")]
    public Spr Action_PivotIn_Left { get; private set; }
    [SpriteLoading("icon", "pawsai")]
    public Spr Action_PivotIn_Right { get; private set; }
    [SpriteLoading("icon", "pawsai")]
    public Spr Action_PivotOut_Left { get; private set; }
    [SpriteLoading("icon", "pawsai")]
    public Spr Action_PivotOut_Right { get; private set; }

    private readonly static List<Type> PawsaiCardTypes = [
        // Common
        typeof(PullShot),
        typeof(Safeguard),
        typeof(RepulsionCard),
        typeof(ShieldRush),
        typeof(DeSync),
        typeof(ShieldShift),
        typeof(ShieldRecovery),
        typeof(Chase),
        typeof(ThrustRedirect),
        typeof(PawsaiExe),
        // Uncommon
        typeof(EmergencyRetreat),
        typeof(SpikedShield),
        typeof(EnergizedMove),
        typeof(ShieldShort),
        typeof(HeavySlugger),
        typeof(HeavyShielding),
        typeof(SpeedChaser),
        // Rare
        typeof(SmartAlignment),
        typeof(Potentiomatic),
        typeof(Ultimatomatic),
        typeof(CautiousRepeater),
        typeof(ShieldPayout),
        // Token
        // Unreleased
    ];
    private readonly static List<Type> PawsaiDuoArtifactTypes = [
    // typeof(),  // CAT
    // typeof(),  // Peri
    // typeof(),  // Isaac
    // typeof(),  // Books
    // typeof(),  // Drake
    // typeof(),  // Dizzy
    // typeof(),  // Riggs
    // typeof(),  // Kodijen
    // typeof(),  // Varrigradona
    // typeof(),  // Bruno
    // typeof(),  // Parmesan
    ];
    private readonly static IEnumerable<Type> PawsaiArtifactTypes =
        new List<Type>
        {
            // Common
            typeof(OffensiveDefense),
            typeof(ManyConsequences),
            typeof(PowerPrep),
            // Boss
            typeof(DelayedStart),
            typeof(PainCruiser)
            // Event
            // Unreleased
        }.Concat(PawsaiDuoArtifactTypes);
    // private readonly static List<Type> PawsaiDialogues = [
    //     typeof(StoryDialogue),
    //     typeof(EventDialogue),
    //     typeof(CombatDialogue),
    //     typeof(ArtifactDialogue),
    //     typeof(CardDialogue),
    //     typeof(MemoryDialogue)
    // ];
    public readonly static Dictionary<int, List<string>> PawsaiAnims = new()
    {
        {1, [
            "mini",
            "gameover",
        ]},
        {5, [
            "squint",
            "neutral",
        ]}
    };
    #endregion

    #region Bruno stuff
    internal IDeckEntry BrunoDeck;
    internal IStatusEntry Status_Recoil { get; private set; }
    internal IStatusEntry Status_Hyperdrive { get; private set; }
    internal IStatusEntry Status_Mitigate { get; private set; }
    internal IStatusEntry Status_SlowBurn { get; private set; }
    internal IStatusEntry Status_Hamper { get; private set; }

    private readonly static List<Type> BrunoCardTypes = [
        // Common
        typeof(Breacher),
        typeof(ToughenUp),
        typeof(HypeUp),
        typeof(AllIn),
        typeof(RepeatedBreach),
        typeof(ForwardsConverter),
        typeof(TouchyTrigger),
        typeof(Swig),
        typeof(SafeChoice),
        // Uncommon
        typeof(MomentaryBoost),
        typeof(Breather),
        typeof(UnexpectedBlast),
        typeof(OptimizedBlast),
        typeof(EpicBreacher),
        typeof(NaggingSafety),
        typeof(PrepWork),
        // Rare
        typeof(HurtBurn),
        typeof(PainfulChoices),
        typeof(DeepClean),
        typeof(RecoilRebound),
        typeof(Opportunist),
        // Token
        typeof(BrunoExe),
        // Unreleased
    ];
    private readonly static List<Type> BrunoDuoArtifactTypes = [
    // typeof(),  // CAT
    // typeof(),  // Peri
    // typeof(),  // Isaac
    // typeof(),  // Books
    // typeof(),  // Drake
    // typeof(),  // Dizzy
    // typeof(),  // Riggs
    // typeof(),  // Kodijen
    // typeof(),  // Varrigradona
    // typeof(),  // PAWSAI
    // typeof(),  // Parmesan
    ];
    private readonly static IEnumerable<Type> BrunoArtifactTypes =
        new List<Type>
        {
            // Common
            typeof(ThermoelectricCannonCooler),
            typeof(UnfairAdvantage),
            typeof(OverwhelmingStrike),
            // Boss
            typeof(DoubleEdgedCannon),
            typeof(ForsakenSafety)
            // Event
            // Unreleased
        }.Concat(BrunoDuoArtifactTypes);
    // private readonly static List<Type> BrunoDialogues = [
    //     typeof(StoryDialogue),
    //     typeof(EventDialogue),
    //     typeof(CombatDialogue),
    //     typeof(ArtifactDialogue),
    //     typeof(CardDialogue),
    //     typeof(MemoryDialogue)
    // ];
    public readonly static Dictionary<int, List<string>> BrunoAnims = new()
    {
        {1, [
            "mini",
            "gameover",
        ]},
        {5, [
            "squint",
            "neutral",
        ]}
    };

    #endregion

    private readonly static IEnumerable<Type> AllRegisterableTypes =
        PawsaiCardTypes
            .Concat(BrunoCardTypes);

}