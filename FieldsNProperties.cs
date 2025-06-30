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

namespace Starhunters;

internal partial class ModEntry : SimpleMod
{
    internal static ModEntry Instance { get; private set; } = null!;
    internal static IPlayableCharacterEntryV2 Pawsai { get; private set; } = null!;  // Defense
    internal static IPlayableCharacterEntryV2 Kodijen { get; private set; } = null!;  // Mid-row
    internal static IPlayableCharacterEntryV2 Varrigradona { get; private set; } = null!;  // ???
    internal static IPlayableCharacterEntryV2 Bauie { get; private set; } = null!;  // Offense
    internal static IPlayableCharacterEntryV2 Parmesan { get; private set; } = null!;  // Movement? (Teleportation)
    internal string UniqueName { get; private set; }
    internal Harmony Harmony;
    internal IKokoroApi KokoroApi;
    internal ILocalizationProvider<IReadOnlyList<string>> AnyLocalizations { get; }
    internal ILocaleBoundNonNullLocalizationProvider<IReadOnlyList<string>> Localizations { get; }
    internal IMoreDifficultiesApi? MoreDifficultiesApi { get; private set; }
    internal IDuoArtifactsApi? DuoArtifactsApi { get; private set; }
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
    internal Spr Action_MirrorMove_Random { get; private set; }
    internal Spr Action_MirrorMove_RandomLeft { get; private set; }
    internal Spr Action_MirrorMove_Zero { get; private set; }
    internal Spr Action_MirrorMove_Left { get; private set; }
    internal Spr Action_MirrorMove_Right { get; private set; }
    internal Spr Action_MirrorMove_RandomFoe { get; private set; }
    internal Spr Action_MirrorMove_ZeroFoe { get; private set; }
    internal Spr Action_MirrorMove_LeftFoe { get; private set; }
    internal Spr Action_MirrorMove_RightFoe { get; private set; }
    internal Spr Action_PivotIn_Zero { get; private set; }
    internal Spr Action_PivotOut_Zero { get; private set; }
    internal Spr Action_PivotIn_Left { get; private set; }
    internal Spr Action_PivotIn_Right { get; private set; }
    internal Spr Action_PivotOut_Left { get; private set; }
    internal Spr Action_PivotOut_Right { get; private set; }
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
    // typeof(),  // Bauie
    // typeof(),  // Parmesan
    ];
    private readonly static IEnumerable<Type> PawsaiArtifactTypes = 
        new List<Type>
        {
            // Common
            // typeof(OffensiveDefense),
            // typeof(ManyConsequences),
            // typeof(PowerPrep),
            // Boss
            // typeof(DelayedStart),
            // typeof(PainCruiser)
            // Event
            // Unreleased
        }.Concat(PawsaiDuoArtifactTypes);
    private readonly static List<Type> PawsaiDialogues = [
        typeof(StoryDialogue),
        typeof(EventDialogue),
        typeof(CombatDialogue),
        typeof(ArtifactDialogue),
        typeof(CardDialogue),
        typeof(MemoryDialogue)
    ];
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



    private readonly static IEnumerable<Type> AllRegisterableTypes =
        PawsaiCardTypes
            .Concat(PawsaiDialogues);

}