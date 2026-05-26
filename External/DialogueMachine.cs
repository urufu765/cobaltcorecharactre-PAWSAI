using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Nanoray.PluginManager;
using Nickel;

namespace Starhunters.External;

/**
ver.0.20c

To get DialogueMachine and the custom dialogue stuff working:
- edit the namespace of this file to at least match your project namespace
- Instantiate LocalDB in ModEntry.cs *after* all the dialogue has been added (or in a helper.Events.OnModLoadFinished AfterDbInit presented as such below):
        helper.Events.OnModLoadPhaseFinished += (_, phase) =>
        {
            if (phase == ModLoadPhase.AfterDbInit)
            {
                localDB = new(helper, package);
            }
        };
- Then register the locale of your dialogue by calling the instantiated LocalDB's GetLocalizationResults() in helper.Events.OnLoadStringsForLocale:
        helper.Events.OnLoadStringsForLocale += (_, thing) =>
        {
            foreach (KeyValuePair<string, string> entry in localDB.GetLocalizationResults(thing.Locale))
            {
                thing.Localizations[entry.Key] = entry.Value;
            }
        };
- You're all set!
(when you're adding dialogue, you should use LocalDB.DumpStoryToLocalLocale())
*/


/// <summary>
/// Enumerator for setting the mode of dialogue.
/// </summary>
public enum DMod
{
    /// <summary>
    /// Normal dialogue mode
    /// </summary>
    dialogue,
    /// <summary>
    /// Switch say mode
    /// </summary>
    switchsay,
    /// <summary>
    /// Place a placeholder (to allow dialogue from an external source to fill in the blank)
    /// </summary>
    retain,
    /// <summary>
    /// Instruction mode
    /// </summary>
    instruction,
    /// <summary>
    /// Title card mode
    /// </summary>
    title,
}

/// <summary>
/// Enumerator for specifying the edit mode
/// </summary>
public enum EMod
{
    /// <summary>
    /// Edit/add to the Nth switchsay starting from the start. First switchsay is 1, second is 2...
    /// </summary>
    countFromStart,
    /// <summary>
    /// Edit/add to the Nth switchsay starting from the end. Last switchsay is 1, second last is 2...
    /// </summary>
    countFromEnd,
    /// <summary>
    /// Find the switchsay based on a Say's hash inside the desired switchsay. May need to be updated if game's locale has changed.
    /// </summary>
    findSwitchWithHash
}


/// <summary>
/// Just a thing that branches into a dialogue object or an edit object.
/// </summary>
public class AbstractThing
{
    public string? who;
    public string? loopTag;
    public string? what;
    public bool flipped;
    public bool ifCrew;
    public double delay;
    public string? choiceFunc;
}

/// <summary>
/// An object purposed to help locate the existing SwitchSay which you are gonna add a dialogue into.
/// </summary>
public class EditThing : AbstractThing
{
    /// <summary>
    /// Which switch to enter new dialogue into? Starts from 1 (not 0!)
    /// </summary>
    public int? switchNumber;
    /// <summary>
    /// Search for switch from start to end, or end to start, or by hash (which is autoset by the appropriate constructor)
    /// </summary>
    public EMod searchConfig;
    /// <summary>
    /// The hash to match in the switch the hash's dialogue belongs to
    /// </summary>
    public string? hashSearch;
    /// <summary>
    /// An edit dialogue with emotions and all, counting from the start or end to find the Nth switch to add the desired dialogue to.
    /// </summary>
    /// <param name="searchConfig">Whether to search from the start or the end (findSwitchWithHash is not a valid config here)</param>
    /// <param name="switchNumber">Counting from 1, the Nth switch to find</param>
    /// <param name="who">Who says the line?</param>
    /// <param name="loopTag">What is their emotion?</param>
    /// <param name="what">What do they say?</param>
    /// <param name="flipped">Flip the dialogue to right side?</param>
    /// <param name="ifCrew"></param>
    /// <param name="delay">How long in seconds to delay the chatter after making the character appear?</param>
    /// <param name="choiceFunc"></param>
    public EditThing(EMod searchConfig, int switchNumber, string who, string loopTag, string what, bool flipped = false, bool ifCrew = false, double delay = 0.0, string? choiceFunc = null)
    {
        this.searchConfig = searchConfig;
        this.who = who;
        this.loopTag = loopTag;
        this.what = what;
        this.flipped = flipped;
        this.ifCrew = ifCrew;
        this.delay = delay;
        this.choiceFunc = choiceFunc;
        this.switchNumber = switchNumber;
    }
    /// <summary>
    /// An edit dialogue with neutral emotion, counting from the start or end to find the Nth switch to add the desired dialogue to.
    /// </summary>
    /// <param name="searchConfig">Whether to search from the start or the end (findSwitchWithHash is not a valid config here)</param>
    /// <param name="switchNumber">Counting from 1, the Nth switch to find</param>
    /// <param name="who">Who says the line?</param>
    /// <param name="what">What do they say?</param>
    /// <param name="flipped">Flip the dialogue to right side?</param>
    /// <param name="ifCrew"></param>
    /// <param name="delay">How long in seconds to delay the chatter after making the character appear?</param>
    /// <param name="choiceFunc"></param>
    public EditThing(EMod searchConfig, int switchNumber, string who, string what, bool flipped = false, bool ifCrew = false, double delay = 0.0, string? choiceFunc = null)
    {
        this.searchConfig = searchConfig;
        this.who = who;
        this.what = what;
        this.flipped = flipped;
        this.ifCrew = ifCrew;
        this.delay = delay;
        this.choiceFunc = choiceFunc;
        this.switchNumber = switchNumber;
    }
    /// <summary>
    /// An edit dialogue with emotions and all, adding the dialogue to the desired switchsay by finding the switch with the specified existing line's hash.
    /// </summary>
    /// <param name="hashToFind">The hash of any existing line that belongs to the switchsay you want to find.</param>
    /// <param name="who">Who says the line?</param>
    /// <param name="loopTag">What is their emotion?</param>
    /// <param name="what">What do they say?</param>
    /// <param name="flipped">Flip the dialogue to right side?</param>
    /// <param name="ifCrew"></param>
    /// <param name="delay">How long in seconds to delay the chatter after making the character appear?</param>
    /// <param name="choiceFunc"></param>
    public EditThing(string hashToFind, string who, string loopTag, string what, bool flipped = false, bool ifCrew = false, double delay = 0.0, string? choiceFunc = null)
    {
        this.searchConfig = EMod.findSwitchWithHash;
        this.who = who;
        this.loopTag = loopTag;
        this.what = what;
        this.flipped = flipped;
        this.ifCrew = ifCrew;
        this.delay = delay;
        this.choiceFunc = choiceFunc;
        this.hashSearch = hashToFind;
    }

    /// <summary>
    /// An edit dialogue with neutral emotions, adding the dialogue to the desired switchsay by finding the switch with the specified existing line's hash.
    /// </summary>
    /// <param name="hashToFind">The hash of any existing line that belongs to the switchsay you want to find.</param>
    /// <param name="who">Who says the line?</param>
    /// <param name="what">What do they say?</param>
    /// <param name="flipped">Flip the dialogue to right side?</param>
    /// <param name="ifCrew"></param>
    /// <param name="delay">How long in seconds to delay the chatter after making the character appear?</param>
    /// <param name="choiceFunc"></param>
    public EditThing(string hashToFind, string who, string what, bool flipped = false, bool ifCrew = false, double delay = 0.0, string? choiceFunc = null)
    {
        this.searchConfig = EMod.findSwitchWithHash;
        this.who = who;
        this.what = what;
        this.flipped = flipped;
        this.ifCrew = ifCrew;
        this.delay = delay;
        this.choiceFunc = choiceFunc;
        this.hashSearch = hashToFind;
    }

    /// <summary>
    /// Create a deep clone of a thing
    /// </summary>
    /// <param name="editor"></param>
    public EditThing(EditThing editor)
    {
        this.who = editor.who;
        this.loopTag = editor.loopTag;
        this.what = editor.what;
        this.flipped = editor.flipped;
        this.ifCrew = editor.ifCrew;
        this.delay = editor.delay;
        this.choiceFunc = editor.choiceFunc;
        this.switchNumber = editor.switchNumber;
        this.searchConfig = editor.searchConfig;
        this.hashSearch = editor.hashSearch;
    }
}

/// <summary>
/// A dialogue object that streamlines adding new dialogue, which on load is converted into a Say or something like that such that it's basically identical as to how existing dialogue objects work.
/// </summary>
public class DialogueThing : AbstractThing
{
    public string? title;
    public List<DialogueThing>? saySwitch;
    public DMod mode;
    public Instruction? instruction;

    /// <summary>
    /// A dialogue with emotions and all
    /// </summary>
    /// <param name="who">Who speaketh?</param>
    /// <param name="loopTag">How emote?</param>
    /// <param name="what">What they sayeth?</param>
    public DialogueThing(string who, string loopTag, string what, bool flipped = false, bool ifCrew = false, double delay = 0.0, string? choiceFunc = null)
    {
        this.mode = DMod.dialogue;
        this.who = who;
        this.loopTag = loopTag;
        this.what = what;
        this.flipped = flipped;
        this.ifCrew = ifCrew;
        this.delay = delay;
        this.choiceFunc = choiceFunc;
    }
    /// <summary>
    /// A dialogue with neutral emotion
    /// </summary>
    /// <param name="who">Who speaketh?</param>
    /// <param name="what">What they sayeth?</param>
    public DialogueThing(string who, string what, bool flipped = false, bool ifCrew = false, double delay = 0.0, string? choiceFunc = null)
    {
        this.mode = DMod.dialogue;
        this.who = who;
        this.what = what;
        this.flipped = flipped;
        this.ifCrew = ifCrew;
        this.delay = delay;
        this.choiceFunc = choiceFunc;
    }
    /// <summary>
    /// Adds a spacer that will allow the original text to fill in if the mod order is suboptimal. ONLY USED FOR ADDING TO EXISTING DIALOGUE.
    /// </summary>
    public DialogueThing()
    {
        this.mode = DMod.retain;
    }
    /// <summary>
    /// For adding any instructions unfulfilled by this dialogue thing
    /// </summary>
    /// <param name="instruction">Instructions to add</param>
    public DialogueThing(Instruction instruction)
    {
        this.mode = DMod.instruction;
        this.instruction = instruction;
    }
    /// <summary>
    /// For adding text to title cards
    /// </summary>
    /// <param name="title">The title to show (NULL for empty=true)</param>
    public DialogueThing(string? title)
    {
        this.mode = DMod.title;
        this.title = title;
    }

    /// <summary>
    /// Practically a SaySwitch. The list cannot contain anything but just dialogue.
    /// </summary>
    /// <param name="saySwitch">A list of Dialogue to go in 'ere</param>
    public DialogueThing(List<DialogueThing> saySwitch)
    {
        this.mode = DMod.switchsay;
        this.saySwitch = saySwitch;
    }

    /// <summary>
    /// For advanced stuff
    /// </summary>
    /// <param name="mode">Mode of dialogue</param>
    /// <param name="who">Whomst'd've</param>
    /// <param name="loopTag">Emotion</param>
    /// <param name="what">What they say?</param>
    /// <param name="flipped">Flipped to other side</param>
    /// <param name="ifCrew">???</param>
    /// <param name="delay">Delay</param>
    /// <param name="choiceFunc">Route choose</param>
    /// <param name="saySwitch">SaySwitch list</param>
    /// <param name="instruction">Custom instruction</param>
    /// <param name="title">Title</param>
    public DialogueThing(DMod mode, string? who = null, string? loopTag = null, string? what = null, bool flipped = false, bool ifCrew = false, double delay = 0.0, string? choiceFunc = null, List<DialogueThing>? saySwitch = null, Instruction? instruction = null, string? title = null)
    {
        this.mode = mode;
        this.who = who;
        this.loopTag = loopTag;
        this.what = what;
        this.flipped = flipped;
        this.ifCrew = ifCrew;
        this.delay = delay;
        this.choiceFunc = choiceFunc;
        this.saySwitch = saySwitch;
        this.instruction = instruction;
        this.title = title;
    }

    /// <summary>
    /// Constructs a new object based on the thing
    /// </summary>
    /// <param name="dialogue">The dialogue thing to copy</param>
    public DialogueThing(DialogueThing dialogue, bool fromSwitch = false)
    {
        this.mode = dialogue.mode;
        this.who = dialogue.who;
        this.loopTag = dialogue.loopTag;
        this.what = dialogue.what;
        this.flipped = dialogue.flipped;
        this.ifCrew = dialogue.ifCrew;
        this.delay = dialogue.delay;
        this.choiceFunc = dialogue.choiceFunc;
        this.title = dialogue.title;
        this.instruction = dialogue.instruction is not null ? Mutil.DeepCopy(dialogue.instruction) : default;

        if (!fromSwitch)
        {
            this.saySwitch = dialogue.saySwitch?.Select(agh => new DialogueThing(agh, true)).ToList();
        }
    }
}

/// <summary>
/// An instruction for writing multiple nodes that share similar or the same filters.
/// DOES NOT SUPPORT EDITS
/// </summary>
public class QMulti : Instruction  // Quick disconnect
{
    /// <summary>
    /// A mode that ignores any filters set by the child and only use the parent filters. Reduces calculations done per QuickMulti
    /// </summary>
    public bool quickMultiMode = false;

    /// <summary>
    /// A storynode that contains filters that will override the parent filters
    /// </summary>
    public DialogueMachine filterOverrides;

    /// <summary>
    /// Quick multi node separator that inherits the parent node's filters
    /// </summary>
    public QMulti()
    {
        quickMultiMode = true;
        filterOverrides = new();
    }

    /// <summary>
    /// Quick multi node separator that only changes the AllPresent list, for dialogue sets that cover multiple character interactions but changes nothing else.
    /// </summary>
    /// <param name="allPresents">AllPresent. Who needs to be present. This isn't a spelling error, this is to remove the ambiguity between this constructor and the many parameter constructor if for some reason you want to specify the parameter name.</param>
    /// <param name="overrides">Delegate for any other filters that is not present in the parameters</param>
    public QMulti(HashSet<string>? allPresents, Action<DialogueMachine>? overrides = null)
    {
        filterOverrides = new()
        {
            allPresent = allPresents
        };
        overrides?.Invoke(filterOverrides);
    }
    /// <summary>
    /// Quick multi node separator that overrides commonly changed filters. Parameters left default or null inherits the parent node's instead.
    /// </summary>
    /// <param name="hasArtifactTypes">Artifacts needed (Type)</param>
    /// <param name="hasArtifacts">Artifacts needed (string)</param>
    /// <param name="doesNotHaveArtifactTypes">Artifacts blacklist (Type)</param>
    /// <param name="doesNotHaveArtifacts">Artifacts blacklist (string)</param>
    /// <param name="allPresent">Who needs to be present</param>
    /// <param name="oncePerCombatTags">Once per combat tag</param>
    /// <param name="oncePerRunTags">Once per run tag</param>
    /// <param name="lastTurnEnemyStatuses">Statuses enemy had</param>
    /// <param name="lastTurnPlayerStatuses">Statuses player had</param>
    /// <param name="overrides">Delegate for any other filters that is not present in the parameters</param>
    public QMulti(HashSet<Type>? hasArtifactTypes = null, HashSet<string>? hasArtifacts = null, HashSet<Type>? doesNotHaveArtifactTypes = null, HashSet<string>? doesNotHaveArtifacts = null, HashSet<string>? allPresent = null, HashSet<string>? oncePerCombatTags = null, HashSet<string>? oncePerRunTags = null, HashSet<Status>? lastTurnEnemyStatuses = null, HashSet<Status>? lastTurnPlayerStatuses = null, Action<DialogueMachine>? overrides = null)
    {
        filterOverrides = new()
        {
            allPresent = allPresent,
            oncePerCombatTags = oncePerCombatTags,
            oncePerRunTags = oncePerRunTags,
            hasArtifactTypes = hasArtifactTypes,
            hasArtifacts = hasArtifacts,
            doesNotHaveArtifactTypes = doesNotHaveArtifactTypes,
            doesNotHaveArtifacts = doesNotHaveArtifacts,
            lastTurnEnemyStatuses = lastTurnEnemyStatuses,
            lastTurnPlayerStatuses = lastTurnPlayerStatuses
        };
        overrides?.Invoke(filterOverrides);
    }

    /// <summary>
    /// For using uncommon filters that differ with the parent node
    /// </summary>
    /// <param name="overrides">Delegate to input any desired DialogueMachine story node filters (which will override the parent filters)</param>
    public QMulti(Action<DialogueMachine>? overrides)
    {
        filterOverrides = new();
        overrides?.Invoke(filterOverrides);
    }
}

/// <summary>
/// The thing that basically takes your dialogue, passes them through a few conversions, stitches trees together, and puts out a StoryNode object which functions just the way the game expects it to.
/// </summary>
public class DialogueMachine : StoryNode
{
    // public List<(string whoOrCommand, string? loopTag, string? what)> dialogue = null!;

    /// <summary>
    /// Edits existing dialogue by finding the switch you want to insert your dialogue into. Best used for vanilla dialogue. WILL IGNORE 'dialogue' DICTIONARY IF 'edit' IS USED
    /// </summary>
    public List<EditThing>? edit;

    /// <summary>
    /// Where all your dialogue *should* go. It can also support titles, mod dialogue edits, and custom instructions!
    /// </summary>
    public List<DialogueThing>? dialogue;

    /// <summary>
    /// Add the type of the artifact rather than trying to use the string key. Gets converted to hasArtifacts later.
    /// </summary>
    public HashSet<Type>? hasArtifactTypes;
    /// <summary>
    /// Add the type of the artifact rather than trying to use the string key. Gets converted to doesNotHaveArtifacts later.
    /// </summary>
    public HashSet<Type>? doesNotHaveArtifactTypes;
    /// <summary>
    /// Though any fields you declare will replace existing fields if you're modifying the original, lists and hashsets will be appended by default. Add the name of the list/hashset field if you want to completely replace them.
    /// </summary>
    public List<string>? dontAppendListFields;

    /// <summary>
    /// Translates DialogueMachine into Instructions readable by LocalDB
    /// </summary>
    public void Convert(SimpleMod inst)
    {
        if (hasArtifactTypes is not null)
        {
            hasArtifacts ??= [];
            foreach (Type type in hasArtifactTypes)
            {
                // Modded
                if (inst.Helper?.Content?.Artifacts?.LookupByArtifactType(type) is IArtifactEntry iae) hasArtifacts.Add(iae.UniqueName);
                else if (DB.artifacts.ContainsValue(type)) hasArtifacts.Add(DB.artifacts.First(x => x.Value == type).Key);
                else inst.Logger.LogWarning($"Error when moving {type.Name} from [hasArtifactTypes] to [hasArtifacts]! Perhaps the artifact isn't registered yet or misspelt?");
            }
        }
        if (doesNotHaveArtifactTypes is not null)
        {
            doesNotHaveArtifacts ??= [];
            foreach (Type type in doesNotHaveArtifactTypes)
            {
                // Modded
                if (inst.Helper?.Content?.Artifacts?.LookupByArtifactType(type) is IArtifactEntry iae) doesNotHaveArtifacts.Add(iae.UniqueName);
                else if (DB.artifacts.ContainsValue(type)) doesNotHaveArtifacts.Add(DB.artifacts.First(x => x.Value == type).Key);
                else inst.Logger.LogWarning($"Error when moving {type.Name} from [doesNotHaveArtifactTypes] to [doesNotHaveArtifacts]! Perhaps the artifact isn't registered yet or misspelt?");
            }
        }
        if (edit is not null)  // Skips dialogue conversion if edits are available
        {
            foreach (EditThing e in edit)
            {
                lines.Add(e.searchConfig switch
                {
                    EMod.countFromStart => new InsertDialogueInSwitch
                    {
                        say = ConvertDialogueToSay(e),
                        whichSwitch = e.switchNumber
                    },
                    EMod.countFromEnd => new InsertDialogueInSwitch
                    {
                        say = ConvertDialogueToSay(e),
                        whichSwitch = e.switchNumber,
                        fromEnd = true
                    },
                    EMod.findSwitchWithHash => new InsertDialogueInSwitch
                    {
                        say = ConvertDialogueToSay(e),
                        whichHash = e.hashSearch
                    },
                    _ => new InsertDialogueInSwitch()  // should never occur
                });
            }
            return;
        }
        foreach (DialogueThing d in dialogue ??= [])
        {
            lines.Add(ConvertDialogueToLine(d));
        }
    }

    /// <summary>
    /// Converts either DialogueThing or EditThing into a Say in a format the LocalDB converter will understand
    /// </summary>
    /// <param name="at"></param>
    /// <returns></returns>
    private static Say ConvertDialogueToSay(AbstractThing at)
    {
        return new Say
        {
            who = at.who ?? "",
            loopTag = at.loopTag,
            hash = at.what ?? "",
            flipped = at.flipped,
            ifCrew = at.ifCrew,
            delay = at.delay,
            choiceFunc = at.choiceFunc
        };
    }

    /// <summary>
    /// Converts a DialogueThing to an instruction so it can go in All[].lines
    /// </summary>
    /// <param name="dt"></param>
    /// <returns></returns>
    private static Instruction ConvertDialogueToLine(DialogueThing dt)
    {
        if (dt.mode == DMod.retain)
        {
            return new RetainOrig();
        }
        if (dt.mode == DMod.instruction && dt.instruction is not null)
        {
            return dt.instruction;
        }
        if (dt.mode == DMod.title)
        {
            if (dt.title is null)
            {
                return new TitleCard { empty = true };
            }
            else
            {
                return new TitleCard { hash = dt.title };
            }
        }
        if (dt.mode == DMod.switchsay && dt.saySwitch is not null)
        {
            SaySwitch ss = new()
            {
                lines = new()
            };
            foreach (DialogueThing dial in dt.saySwitch)
            {
                if (dial.mode == DMod.dialogue) ss.lines.Add(ConvertDialogueToSay(dial));
            }
            return ss;
        }

        return ConvertDialogueToSay(dt);
    }

    /// <summary>
    /// Eventually find a better way of tackling this (UNUSED)
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static bool CharExists(string name, SimpleMod inst)
    {
        if (inst.Helper.Content?.Decks?.LookupByUniqueName(name) is not null) return true;

        if (DB.currentLocale.strings.ContainsKey("char." + name)) return true;  // this probably doesn't even work

        // if (ModEntry.Instance.Helper.Content?.Decks?.LookupByUniqueName($"{ModEntry.Instance.UniqueName}::{name}") is not null)
        // {
        //     return true;
        // }
        return false;
    }

    /// <summary>
    /// Checks if inputted string is a valid Artifact
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static bool ArtifactExists(string name, SimpleMod inst)
    {
        // Modded artifacts
        if (inst.Helper.Content?.Artifacts?.LookupByUniqueName(name) is not null) return true;
        // Game artifacts
        if (DB.artifacts.ContainsKey(name)) return true;
        return false;
    }
}


/// <summary>
/// Puts a placeholder for the original dialogue you're editing to fill in in that very spot.
/// </summary>
public class RetainOrig : Instruction
{
    public override string ToString()
    {
        return "RetainPlease";
    }
}

/// <summary>
/// Inserts a Say in the switch you're looking for
/// </summary>
public class InsertDialogueInSwitch : Instruction
{
    public Say say = null!;
    public int? whichSwitch;
    public bool fromEnd;
    public string? whichHash;
}


/// <summary>
/// Where the custom dialogue is saved, and shoved into the game at load
/// </summary>
public class LocalDB
{
    /// <summary>
    /// Default custom dialogue
    /// </summary>
    [Obsolete]
    public static Story LocalStory { get; set; } = new();
    /// <summary>
    /// Coded custom dialogue for different locales. Please use DumpStoryToLocalLocale() to add your dialogue safely instead!
    /// </summary>
    public static Dictionary<string, Story> LocalStoryLocale { get; private set; } = new();
    /// <summary>
    /// Coded custom dialogue specifically for modded support. Please use DumpStoryToLocalLocale() to add your dialogue safely instead
    /// </summary>
    public static Dictionary<string, Dictionary<string, Story>> ModdedStoryLocale { get; private set; } = new();
    private int incrementingHash = 1;
    /// <summary>
    /// An incrementing hash. WARNING: Hash may conflict if under the same namespace!
    /// </summary>
    public int IncrementingHash
    {
        get => incrementingHash++;
    }

    /// <summary>
    /// The localization dictionary that contains each translation dictionary with the locale as the key.
    /// </summary>
    private readonly Dictionary<string, Dictionary<string, string>> localLocalization;

    /// <summary>
    /// A catalogue of hashes to be shared across languages
    /// </summary>
    private readonly Dictionary<string, List<string>> hashCatalogue;

    /// <summary>
    /// Change ModEntry.Instance if necessary
    /// </summary>
    private static SimpleMod Inst => ModEntry.Instance;

    /// <summary>
    /// The default locale that will be also loaded for missing nodes of current locale
    /// </summary>
    public string DefaultLocale { get; private set; }

    /// <summary>
    /// Should be instantiated *after* all the dialogues have been registered OR at Events.OnModLoadPhaseFinished, AfterDbInit.
    /// </summary>
    /// <param name="defaultLocale">The default language the dialogue will fall back to when encountering a missing string on the other language.</param>
    public LocalDB(IModHelper helper, IPluginPackage<IModManifest> package, string defaultLocale = "en")
    {
        localLocalization = new();
        hashCatalogue = new();
        DefaultLocale = defaultLocale;
        foreach (string key in LocalStoryLocale.Keys)
        {
            localLocalization.Add(key, new());
            Story toUseStory = LocalStoryLocale[key];
            foreach (KeyValuePair<string, Dictionary<string, Story>> thing in ModdedStoryLocale)
            {
                if (helper.ModRegistry.LoadedMods.ContainsKey(thing.Key) && ModdedStoryLocale[thing.Key].TryGetValue(key, out Story? value))
                {
                    foreach (KeyValuePair<string, StoryNode> thing2 in value.all)
                    {
                        if (!toUseStory.all.TryAdd(thing2.Key, thing2.Value))
                        {
                            Inst.Logger.LogWarning("Could not add dialogue: " + thing2.Key);
                        }
                    }
                }
            }
            PasteToDB(toUseStory, DB.story, key);
        }
        // TODO: I'll re-implement .json support in the future
        // if (LocalStoryLocale.ContainsKey(DB.currentLocale.locale))  // For other coded translated dialogues
        // {
        // }
        // else if (File.Exists($"{package.PackageRoot}\\i18n\\{DB.currentLocale.locale}_story.json"))  // For i18n translated story dialogue
        // {
        //     toUseStory = Mutil.LoadJsonFile<Story>(package.PackageRoot.GetRelativeFile($"i18n/{DB.currentLocale.locale}_story.json").FullName);
        // }
        // else  // For default
        // {
        //     toUseStory = LocalStory;
        // }

    }

    /// <summary>
    /// This one must be used in Events.OnLoadStringsForLocale.
    /// </summary>
    /// <returns>The localised strings for the appropriate locale</returns>
    public Dictionary<string, string> GetLocalizationResults(string locale)
    {
        Dictionary<string, string> localisationToPresent = [];
        List<string> missingThings = [];
        List<string> extraThings = [];
        bool missingDefault = false;
        bool debugMode = false;
        if (!localLocalization.ContainsKey(DefaultLocale))
        {
            missingDefault = true;
            Inst.Logger.LogWarning("Missing default locale language! Change the default (set in the LocalDB constructor's parameter) or check if you misplaced the dialogues somewhere.");
        }

        // Apply localised dialogue if present, default if not.
        // Regular mode just uses the default as base then 
#if DEBUG
        debugMode = true;

        if (locale == DefaultLocale && !missingDefault)
        {
            localisationToPresent = localLocalization[locale];
        }
        // Debug mode does not use the default locale as backup to make sure you find those missing strings.
        else if (localLocalization.TryGetValue(locale, out Dictionary<string, string>? newLocalisation))
        {
            if (!missingDefault)
            {
                // Go through the default locale to check which node keys are present in the new locale, and which are missing
                foreach (string origLocaleKey in localLocalization[DefaultLocale].Keys)
                {
                    if (newLocalisation.TryGetValue(origLocaleKey, out string? origValue))
                    {
                        localisationToPresent[origLocaleKey] = origValue;
                    }
                    else
                    {
                        missingThings.Add(origLocaleKey);
                    }
                }
                // Go though the new locale to check which node keys are added on as extras, and aren't found in default.
                foreach (KeyValuePair<string, string> newLocale in newLocalisation)
                {
                    if (!localLocalization[DefaultLocale].ContainsKey(newLocale.Key))
                    {
                        extraThings.Add(newLocale.Key);
                        localisationToPresent[newLocale.Key] = newLocale.Value;
                    }
                }
            }
            else
            {
                localisationToPresent = newLocalisation;
            }
        }
        else  // If the desired locale is straight up non-existent, just load the default. There's no point in missingString errors if you are fully aware there isn't even a present string lol
        {
            Inst.Logger.LogWarning("No {locale} found! Loading the default locale {DefaultLocale} if present.", locale, DefaultLocale);
            if (!missingDefault)
            {
                localisationToPresent = localLocalization[DefaultLocale];
            }
        }

        if (!missingDefault && missingThings.Count > 0)
        {
            Inst.Logger.LogWarning("Missing following dialogue nodes from {locale} when comparing to {DefaultLocale}!!!", locale, DefaultLocale);
            Inst.Logger.LogWarning("[{}]", string.Join(", ", missingThings.Select(k => string.Join(":", k.Split(":").Where((_, index) => index != 1)))));
            // The localisation key is made up of NODE_KEY:CLASS_FULLNAME:AUTOINDEX. The middle bit isn't necessary information to the modder, so the string is trimmed accordingly.
        }

        if (!missingDefault && extraThings.Count > 0)
        {
            Inst.Logger.LogWarning("Found extra dialogue nodes not found in {locale} when comparing to {DefaultLocale}!!!", locale, DefaultLocale);
            Inst.Logger.LogWarning("[{}]", string.Join(", ", extraThings.Select(k => string.Join(":", k.Split(":").Where((_, index) => index != 1)))));
        }
#endif

        if (!debugMode)
        {
            if (!missingDefault)
            {
                localisationToPresent = localLocalization[DefaultLocale];  // Default for background
                if (localLocalization.TryGetValue(locale, out Dictionary<string, string>? newLocaleDict))
                {
                    foreach (KeyValuePair<string, string> localisedPair in newLocaleDict)
                    {
                        // Then just replace the localised stuff if present
                        localisationToPresent[localisedPair.Key] = localisedPair.Value;
                    }
                }
            }
            else if (localLocalization.TryGetValue(locale, out Dictionary<string, string>? value))  // No default dialogue found
            {
                localisationToPresent = value;
            }
        }
        return localisationToPresent;
    }

    /// <summary>
    /// A modded version that adds separate dictionaries for each mod key so they could be included/excluded on load
    /// </summary>
    /// <param name="locale">The language the dialogue is written in. 'en' - American, 'es' - Español con vosotros, 'fr' - French, etc.</param>
    /// <param name="modKey">The uniqueName of the mod</param>
    /// <param name="storyStuff">A dictionary containing pairs of node key and DialogueMachine</param>
    public static void DumpStoryToLocalLocale(string locale, string modKey, Dictionary<string, DialogueMachine> storyStuff)
    {
        // Just to trigger the modded part on load lol
        if (!LocalStoryLocale.ContainsKey(locale))
        {
            LocalStoryLocale[locale] = new Story();
        }


        if (!ModdedStoryLocale.ContainsKey(modKey))
        {
            ModdedStoryLocale[modKey] = new();
        }
        if (!ModdedStoryLocale[modKey].ContainsKey(locale))
        {
            ModdedStoryLocale[modKey][locale] = new Story();
        }

        foreach (KeyValuePair<string, DialogueMachine> dm in storyStuff)
        {
            //ExistenceChecker(dm);

            // Tries to add the dialogue in the local locale local locale thing
            if (!ModdedStoryLocale[modKey][locale].all.TryAdd(dm.Key, dm.Value))
            {
                Inst.Logger.LogWarning("Could not add dialogue: " + dm.Key);
            }
        }
    }


    /// <summary>
    /// Adds a list of DialogueMachines to the appropriate locale, creating a new if locale doesn't exist.
    /// </summary>
    /// <param name="locale">the locale the storynodes will go in. 'en' - Freedom English, 'es' - Lingua de España, 'fr' - French, etc.</param>
    /// <param name="storyStuff">Storynodes along with their keys to add to locale dictionary</param>
    public static void DumpStoryToLocalLocale(string locale, Dictionary<string, DialogueMachine> storyStuff)
    {
        if (!LocalStoryLocale.ContainsKey(locale))
        {
            LocalStoryLocale[locale] = new Story();
        }

        foreach (KeyValuePair<string, DialogueMachine> dm in storyStuff)
        {
            // Tries to add the dialogue in the local locale local locale thing
            if (!LocalStoryLocale[locale].all.TryAdd(dm.Key, dm.Value))
            {
                Inst.Logger.LogWarning("Could not add dialogue: " + dm.Key);
            }
        }
    }

    /// <summary>
    /// Checks the existence of things, and warns the MODDER appropriately. On release builds the warnings will not show up.
    /// </summary>
    /// <param name="sn"></param>
    private static void ExistenceChecker(KeyValuePair<string, StoryNode> sn)
    {
#if DEBUG
        // Checks if the inputted artifact is a valid one that the game can check
        if (sn.Value.hasArtifacts is not null)
        {
            foreach (string artifact in sn.Value.hasArtifacts)
            {
                if (!DialogueMachine.ArtifactExists(artifact, Inst))
                {
                    Inst.Logger.LogWarning(sn.Key + "'s <hasArtifacts> may contain an erroneous artifact [" + artifact + "] that may not be recognized by the game! (or if it's a modded artifact: the mod isn't loaded.)");
                }
            }
        }
        if (sn.Value.doesNotHaveArtifacts is not null)
        {
            foreach (string artifact in sn.Value.doesNotHaveArtifacts)
            {
                if (!DialogueMachine.ArtifactExists(artifact, Inst))
                {
                    Inst.Logger.LogWarning(sn.Key + "'s <doesNotHaveArtifacts> may contain an erroneous artifact [" + artifact + "] that may not be recognized by the game! (or if it's a modded artifact: the mod isn't loaded.)");
                }
            }
        }

        // Checks if the inputted character is a valid one
        if (sn.Value.allPresent is not null)
        {
            foreach (string characer in sn.Value.allPresent)
            {
                if (!DialogueMachine.CharExists(characer, Inst))
                {
                    Inst.Logger.LogWarning(sn.Key + "'s <allPresent> may contain an erroneous character [" + characer + "] that may not be recognized by the game! (or if it's a modded character: the mod isn't loaded or is a modded enemy.)");
                }
            }
        }
        if (sn.Value.nonePresent is not null)
        {
            foreach (string characer in sn.Value.nonePresent)
            {
                if (!DialogueMachine.CharExists(characer, Inst))
                {
                    Inst.Logger.LogWarning(sn.Key + "'s <nonePresent> may contain an erroneous character [" + characer + "] that may not be recognized by the game! (or if it's a modded character: the mod isn't loaded or is a modded enemy.)");
                }
            }
        }

        if (sn.Value is DialogueMachine dm)
        {
            // Checks whether the who part in the edit or dialogues is not typo-d
            if (dm.edit is not null)
            {
                foreach (EditThing et in dm.edit)
                {
                    if (et.who is not null && !DialogueMachine.CharExists(et.who, Inst))
                    {
                        Inst.Logger.LogWarning(sn.Key + "'s <edit> contains a line with an invalid character [" + et.who + "] that may not be recognized by the game! Did you spell the character correctly? (or if it's a modded character: the mod isn't loaded or is a modded enemy.)");
                    }
                }
            }
            else if (dm.dialogue is not null)
            {
                foreach (DialogueThing dt in dm.dialogue)
                {
                    if (dt.saySwitch is not null)
                    {
                        foreach (DialogueThing dtss in dt.saySwitch)
                        {
                            if (dtss.who is not null && !DialogueMachine.CharExists(dtss.who, Inst))
                            {
                                Inst.Logger.LogWarning(sn.Key + "'s <dialogue(sayswitch)> contains a line with an invalid character [" + dtss.who + "] that may not be recognized by the game! Did you spell the character correctly? (or if it's a modded character: the mod isn't loaded or is a modded enemy.)");
                            }
                        }
                    }
                    else if (dt.who is not null && !DialogueMachine.CharExists(dt.who, Inst))
                    {
                        Inst.Logger.LogWarning(sn.Key + "'s <dialogue> contains a line with an invalid character [" + dt.who + "] that may not be recognized by the game! Did you spell the character correctly? (or if it's a modded character: the mod isn't loaded or is a modded enemy.)");
                    }
                }
            }

            // Checks if the edit dialogue thing's key is valid
            if (dm.edit is not null && !DB.story.all.ContainsKey(sn.Key))
            {
                Inst.Logger.LogWarning(sn.Key + " is trying to add to a dialogue that doesn't exist in game (yet)! If you're trying to edit modded dialogue, this may not be the appropriate way!");
            }
        }
#endif
    }

    /// <summary>
    /// Copies the storynodes from from to to, while also converting it for the game to recognise and registering the locales.
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    private void PasteToDB(Story from, Story to, string locale)
    {
        foreach (KeyValuePair<string, StoryNode> sn in from.all)
        {
            bool editMode = false;
            // Convert all custom DialogueThings from DialogueMachine to StoryNode lines
            if (sn.Value is DialogueMachine dm)
            {
                ExistenceChecker(sn);
                if (dm.dialogue is not null && dm.dialogue.Count > 0 && dm.dialogue[0].instruction is QMulti)
                {
                    Story multiStory = new();
                    List<DialogueMachine> ldm = new();
                    foreach (DialogueThing dt in dm.dialogue)
                    {
                        // Creating a new node with the parent node as the base for filters, and the child node overriding the base if any filters are specified.
                        if (dt.instruction is QMulti qm)
                        {
                            if (qm.quickMultiMode)  // For if there's no filter overrides in place
                            {
                                ldm.Add(NodeCopier(dm, "lines", "edit", "dialogue"));
                            }
                            else
                            {
                                ldm.Add(NodeZipper(dm, qm.filterOverrides, false, "lines", "edit", "dialogue")); // Copy the dialogue machine and merge it with overriding filters
                            }
                            continue;
                        }

                        // add the remaining dialogue objects into each node divided by the QM
                        if (ldm.Count > 0 && ldm[^1] is DialogueMachine dmm)
                        {
                            dmm.dialogue ??= [];
                            dmm.dialogue.Add(dt);
                        }
                    }

                    // Give each new node an automatic name and add them into the DB.Story
                    for (int i = 0; i < ldm.Count; i++)
                    {
                        multiStory.all.Add(sn.Key + "_Multi_" + i, ldm[i]);
                    }
                    PasteToDB(multiStory, DB.story, locale);
                    continue;
                }
                dm.Convert(Inst);
                editMode = dm.edit is not null;
            }

            // If dialogue node already exists in the LOCAL catalogue, meaning a different locale with the same keys may be loading in.
            // Basically, this is to load all the other strings for other languages the mod might add.
            if (hashCatalogue.ContainsKey(sn.Key))
            {
                if (editMode)  // If editing dialogues
                {
                    int i = 0;
                    foreach (Instruction instruction in sn.Value.lines)
                    {
                        if (instruction is InsertDialogueInSwitch idis)
                        {
                            if (i < hashCatalogue[sn.Key].Count)
                            {
                                SetAnotherLocaleFromIDIS(idis, hashCatalogue[sn.Key][i], locale);
                                i++;
                            }
                        }
                    }
                }
                else  // If just adding dialogue. Should work fine even if it only guesses how the dialogue will be thrown in, based on the dialogue order
                {
                    int j = 0;
                    foreach (Instruction instruction in sn.Value.lines)
                    {
                        if (j < hashCatalogue[sn.Key].Count)
                        {
                            if (instruction is Say s)
                            {
                                SetLinesRecognize(s, hashCatalogue[sn.Key][j], locale);
                                j++;
                            }
                            else if (instruction is SaySwitch ss)
                            {
                                foreach (Say sss in ss.lines)
                                {
                                    if (j < hashCatalogue[sn.Key].Count)
                                    {
                                        SetLinesRecognize(sss, hashCatalogue[sn.Key][j], locale);
                                        j++;
                                    }
                                }
                            }
                            else if (instruction is TitleCard t && t.empty is not true)
                            {
                                SetLinesRecognize(t, hashCatalogue[sn.Key][j], locale);
                                j++;
                            }
                        }
                    }
                }
                continue;
            }
            else  // Create a new entry in the catalogue to basically tell the method to only load the localisations if it encounters the same dialogue key from another locale.
            {
                hashCatalogue[sn.Key] = [];
            }

            // Adding nodes that don't exist in the catalogue yet

            // Add dialogue to an existing switch
            if (editMode)
            {
                to.all[sn.Key] = InjectALineIn(sn.Value, to.all[sn.Key], sn.Key, locale);
                continue;
            }


            // Copy storynodes from from to to
            if (to.all.ContainsKey(sn.Key))
            {
                to.all[sn.Key] = StitchNodesTogether(sn.Value, to.all[sn.Key], sn.Key, locale);
            }
            else
            {
                for (int a = 0; a < sn.Value.lines.Count; a++)
                {
                    MakeLinesRecognisable(sn.Value.lines[a], sn.Key, locale);
                }
                to.all.Add(sn.Key, sn.Value);
            }
        }
    }


    /// <summary>
    /// Safely inject a dialogue in an existing dialogue switch
    /// </summary>
    /// <param name="newStory">The freshly cut node to take dialogues from</param>
    /// <param name="existingStory">The existing node to reference</param>
    /// <param name="script">The node key</param>
    /// <returns>StoryNode with the injected dialogue</returns>
    private StoryNode InjectALineIn(in StoryNode newStory, in StoryNode existingStory, string script, string locale)
    {
        try
        {
            StoryNode result = existingStory;
            if (result.lines is not null)
            {
                foreach (Instruction instruction in newStory.lines)
                {
                    if (instruction is InsertDialogueInSwitch idis)
                    {
                        for (int a = 0, b = 0, c = result.lines.Count - 1; b < result.lines.Count && c >= 0; b++, c--)
                        {
                            // Checking from the front (for both counting and searching hash)
                            if (!idis.fromEnd && result.lines[b] is SaySwitch ss)
                            {
                                a++;
                                // Finding switch by matching a hash in every dialogue in the switch
                                if (idis.whichHash is not null)
                                {
                                    foreach (Say say in ss.lines)
                                    {
                                        if (say.hash == idis.whichHash)
                                        {
                                            ss.lines.Add(GetSayFromIDIS(idis, script, locale));
                                            goto endofloop;
                                        }
                                    }
                                }
                                // Finding switch by counting
                                else if (idis.whichSwitch is not null && a == idis.whichSwitch)
                                {
                                    ss.lines.Add(GetSayFromIDIS(idis, script, locale));
                                    goto endofloop;
                                }
                            }
                            // Checking from the end (only for counting)
                            else if (idis.fromEnd && result.lines[c] is SaySwitch bs)
                            {
                                a++;
                                if (idis.whichSwitch is not null && a == idis.whichSwitch)
                                {
                                    bs.lines.Add(GetSayFromIDIS(idis, script, locale));
                                    goto endofloop;
                                }
                            }
                        }
                        Inst.Logger.LogWarning(script + "'s IDIS failed to find a switch to insert the dialogue into!");
                    }
                endofloop:;
                }
            }
            // CombineFields(ref result, newStory);
            result = NodeZipper(result, newStory);
            return result;
        }
        catch (Exception err)
        {
            Inst.Logger.LogError(err, "Failed to edit a line with key:" + script);
            return existingStory;
        }
    }


    /// <summary>
    /// Grabs the Say from IDIS, then converts the Say to be game recognisable as well as registering the locale.
    /// </summary>
    /// <param name="idis"></param>
    /// <param name="script"></param>
    /// <returns></returns>
    private Say GetSayFromIDIS(InsertDialogueInSwitch idis, string script, string locale)
    {
        string what = idis.say.hash;
        idis.say.hash = $"{GetType().FullName}:{IncrementingHash}";
        localLocalization[locale][$"{script}:{idis.say.hash}"] = what;
        hashCatalogue[script].Add($"{script}:{idis.say.hash}");
        return idis.say;
    }


    /// <summary>
    /// Assuming the node and hash already exists, adds the new line extracted from IDIS into the desired locale using the hash.
    /// </summary>
    /// <param name="idis"></param>
    /// <param name="hash"></param>
    /// <param name="locale"></param>
    private void SetAnotherLocaleFromIDIS(InsertDialogueInSwitch idis, string hash, string locale)
    {
        string what = idis.say.hash;
        localLocalization[locale][hash] = what;
    }


    /// <summary>
    /// Combines two storynodes together, automatically determining which of the two are original text based on the existence of RetainOrig
    /// </summary>
    /// <param name="newStory"></param>
    /// <param name="existingStory"></param>
    /// <param name="script"></param>
    /// <returns></returns>
    private StoryNode StitchNodesTogether(in StoryNode newStory, in StoryNode existingStory, string script, string locale)
    {
        try
        {
            StoryNode result = existingStory;  // Assumes that the existing story lines has already been converted from DialogueMachine recognized say to game recognized say
            if (existingStory.lines is not null)
            {
                StoryNode start = newStory;

                for (int x = 0; x < result.lines.Count && x < start.lines.Count; x++)
                {
                    if (result.lines[x].ToString() == "RetainPlease")
                    {
                        MakeLinesRecognisable(start.lines[x], script, locale);
                        result.lines[x] = start.lines[x];
                    }
                    else if (result.lines[x] is Say or SaySwitch && start.lines[x] is Say or SaySwitch)
                    {
                        result.lines[x] = CombineTwoSays(result.lines[x], start.lines[x], script, locale);
                    }
                }

                if (start.lines.Count > result.lines.Count)
                {
                    for (int y = result.lines.Count; y < start.lines.Count; y++)
                    {
                        MakeLinesRecognisable(start.lines[y], script, locale);
                        result.lines.Add(start.lines[y]);
                    }
                }
                result = NodeZipper(result, start);
                //CombineFields(ref result, start);
            }
            return result;
        }
        catch (Exception err)
        {
            Inst.Logger.LogError(err, "Failed to edit a line with key:" + script + " from locale:" + locale);
            return existingStory;
        }
    }

    /// <summary>
    /// Combines two says, resulting in a SaySwitch
    /// </summary>
    /// <param name="existingLine"></param>
    /// <param name="newLine"></param>
    /// <param name="script"></param>
    /// <returns></returns>
    private SaySwitch CombineTwoSays(Instruction existingLine, Instruction newLine, string script, string locale)
    {
        SaySwitch result = new SaySwitch
        {
            lines = new()
        };
        if (existingLine is Say sayA)
        {
            result.lines.Add(sayA);
        }
        else if (existingLine is SaySwitch saySA)
        {
            result = saySA;
        }

        if (newLine is Say sayB)
        {
            MakeLinesRecognisable(sayB, script, locale);
            result.lines.Add(sayB);
        }
        else if (newLine is SaySwitch saySB)
        {
            MakeLinesRecognisable(saySB, script, locale);
            foreach (Say s in saySB.lines)
            {
                result.lines.Add(s);
            }
        }
        return result;
    }

    /// <summary>
    /// Converts a Say, SaySwitch, or Title's hash into a generated one, while adding to the localisation.
    /// </summary>
    /// <param name="instruction"></param>
    /// <param name="script"></param>
    public void MakeLinesRecognisable(Instruction instruction, string script, string locale)
    {
        if (instruction is Say say)
        {
            string what = say.hash;
            say.hash = $"{GetType().FullName}:{IncrementingHash}";
            localLocalization[locale][$"{script}:{say.hash}"] = what;
            hashCatalogue[script].Add($"{script}:{say.hash}");
        }
        else if (instruction is SaySwitch saySwitch)
        {
            for (int a = 0; a < saySwitch.lines.Count; a++)
            {
                MakeLinesRecognisable(saySwitch.lines[a], script, locale);
            }
        }
        else if (instruction is TitleCard title)
        {
            string what = title.hash;
            title.hash = $"{GetType().FullName}:{IncrementingHash}";
            if (title.empty is not true)
            {
                localLocalization[locale][$"{script}:{title.hash}"] = what;
                hashCatalogue[script].Add($"{script}:{title.hash}");
            }
        }
    }

    /// <summary>
    /// Something that adds dialogue to existing hashes. Unlike MakeLinesRecognisable, must be processed into individual says and filtered out empty titles beforehand!
    /// </summary>
    /// <param name="instruction"></param>
    /// <param name="hash"></param>
    /// <param name="locale"></param>
    public void SetLinesRecognize(Instruction instruction, string hash, string locale)
    {
        if (instruction is Say say)
        {
            string what = say.hash;
            localLocalization[locale][hash] = what;
        }
        else if (instruction is TitleCard title)
        {
            string what = title.hash;
            localLocalization[locale][hash] = what;
        }
    }

    /// <summary>
    /// Zips two DialogueMachines together, with child overriding parent where applicable. List/Hashset fields are appended unless specified in the child DialogueMachine's replaceFields.
    /// </summary>
    /// <param name="parent">Parent DialogueMachine</param>
    /// <param name="child">Child DialogueMachine</param>
    /// <param name="excludeFields">Fields to skip</param>
    /// <returns>New DialogueMachine with copied fields</returns>
    public static DialogueMachine NodeZipper(in DialogueMachine parent, in DialogueMachine child, bool appendLists = true, params string[] excludeFields)
    {
        DialogueMachine? result = NodeZipper(parent, child, appendLists, child.dontAppendListFields, excludeFields) as DialogueMachine;
        DialogueMachine man = NodeCopier(parent, excludeFields);

        if (result is not null)
        {
            DialogueMachine kiddo = NodeCopier(child, excludeFields);
            result.dialogue = kiddo.dialogue ?? man.dialogue;
            result.edit = kiddo.edit ?? man.edit;
            result.hasArtifactTypes = kiddo.hasArtifactTypes ?? man.hasArtifactTypes;
            result.doesNotHaveArtifactTypes = kiddo.doesNotHaveArtifactTypes ?? man.doesNotHaveArtifactTypes;
            result.dontAppendListFields = kiddo.dontAppendListFields ?? man.dontAppendListFields;
            if (appendLists)
            {
                if (!excludeFields.Contains("dialogue")) result.dialogue = [.. man.dialogue ?? [], .. child.dialogue ?? []];
                if (!excludeFields.Contains("edit")) result.edit = [.. man.edit ?? [], .. child.edit ?? []];
                if (!excludeFields.Contains("hasArtifactTypes")) result.hasArtifactTypes = [.. man.hasArtifactTypes ?? [], .. child.hasArtifactTypes ?? []];
                if (!excludeFields.Contains("doesNotHaveArtifactTypes")) result.doesNotHaveArtifactTypes = [.. man.doesNotHaveArtifactTypes ?? [], .. child.doesNotHaveArtifactTypes ?? []];
            }
        }
        return result ?? new();
    }

    /// <summary>
    /// Zips two StoryNodes together, with child overriding parent where applicable. List/Hashset fields are appended unless specified in dontAppendFields. 
    /// Pain, and misery.
    /// </summary>
    /// <param name="parent">Parent StoryNode</param>
    /// <param name="child">Child StoryNode</param>
    /// <param name="dontAppendFields">List/Hashsets to override rather than append</param>
    /// <param name="excludeFields">Fields to skip</param>
    /// <returns>New StoryNode with copied fields</returns>
    public static StoryNode NodeZipper(in StoryNode parent, in StoryNode child, bool appendLists = true, List<string>? dontAppendFields = null, params string[] excludeFields)
    {
        StoryNode result = new DialogueMachine();
        StoryNode original = new();
        StoryNode kiddo = NodeCopier(child, excludeFields);
        StoryNode man = NodeCopier(parent, excludeFields);

        result.allPresent = kiddo.allPresent ?? man.allPresent;  //
        result.anyDrones = kiddo.anyDrones ?? man.anyDrones;  //
        result.anyDronesFriendly = kiddo.anyDronesFriendly ?? man.anyDronesFriendly;  //
        result.anyDronesHostile = kiddo.anyDronesHostile ?? man.anyDronesHostile;  //
        result.bg = kiddo.bg ?? man.bg;
        result.bgSetup = kiddo.bgSetup ?? man.bgSetup;  //
        result.canSpawnOnMap = kiddo.canSpawnOnMap ?? man.canSpawnOnMap;
        result.choiceFunc = kiddo.choiceFunc ?? man.choiceFunc;
        result.choiceText = kiddo.choiceText ?? man.choiceText;
        result.demo = kiddo.demo ?? man.demo;
        result.doesNotHaveArtifacts = kiddo.doesNotHaveArtifacts ?? man.doesNotHaveArtifacts;  //
        result.dontCountForProgression = original.dontCountForProgression == kiddo.dontCountForProgression ? man.dontCountForProgression : kiddo.dontCountForProgression;
        result.enemyDoesNotHavePart = kiddo.enemyDoesNotHavePart ?? man.enemyDoesNotHavePart;
        result.enemyHasArmoredPart = kiddo.enemyHasArmoredPart ?? man.enemyHasArmoredPart;
        result.enemyHasBrittlePart = kiddo.enemyHasBrittlePart ?? man.enemyHasBrittlePart;
        result.enemyHasPart = kiddo.enemyHasPart ?? man.enemyHasPart;
        result.enemyHasWeakPart = kiddo.enemyHasWeakPart ?? man.enemyHasWeakPart;
        result.enemyIntent = kiddo.enemyIntent ?? man.enemyIntent;
        result.enemyShotJustHit = kiddo.enemyShotJustHit ?? man.enemyShotJustHit;
        result.enemyShotJustMissed = kiddo.enemyShotJustMissed ?? man.enemyShotJustMissed;
        result.excludedScenes = kiddo.excludedScenes ?? man.excludedScenes;  //
        result.goingToOverheat = kiddo.goingToOverheat ?? man.goingToOverheat;
        result.handEmpty = kiddo.handEmpty ?? man.handEmpty;
        result.handFullOfTrash = kiddo.handFullOfTrash ?? man.handFullOfTrash;
        result.handFullOfUnplayableCards = kiddo.handFullOfUnplayableCards ?? man.handFullOfUnplayableCards;
        result.hasArtifacts = kiddo.hasArtifacts ?? man.hasArtifacts;  //
        result.introDelay = kiddo.introDelay ?? man.introDelay;
        result.justOverheated = kiddo.justOverheated ?? man.justOverheated;
        result.lastDeathZone = kiddo.lastDeathZone ?? man.lastDeathZone;
        result.lastNamedDroneDestroyed = kiddo.lastNamedDroneDestroyed ?? man.lastNamedDroneDestroyed;
        result.lastNamedDroneSpawned = kiddo.lastNamedDroneSpawned ?? man.lastNamedDroneSpawned;
        result.lastTurnEnemyStatuses = kiddo.lastTurnEnemyStatuses ?? man.lastTurnEnemyStatuses;  //
        result.lastTurnPlayerStatuses = kiddo.lastTurnPlayerStatuses ?? man.lastTurnPlayerStatuses;  //
        result.lines = original.lines == kiddo.lines ? man.lines : kiddo.lines;  //
        result.lookup = kiddo.lookup ?? man.lookup;  //
        result.maxCostOfCardJustPlayed = kiddo.maxCostOfCardJustPlayed ?? man.maxCostOfCardJustPlayed;
        result.maxDamageBlockedByEnemyArmorThisTurn = kiddo.maxDamageBlockedByEnemyArmorThisTurn ?? man.maxDamageBlockedByEnemyArmorThisTurn;
        result.maxDamageDealtToEnemyThisAction = kiddo.maxDamageDealtToEnemyThisAction ?? man.maxDamageDealtToEnemyThisAction;
        result.maxDamageDealtToPlayerThisTurn = kiddo.maxDamageDealtToPlayerThisTurn ?? man.maxDamageDealtToPlayerThisTurn;
        result.maxHull = kiddo.maxHull ?? man.maxHull;
        result.maxHullPercent = kiddo.maxHullPercent ?? man.maxHullPercent;
        result.maxTurnsThisCombat = kiddo.maxTurnsThisCombat ?? man.maxTurnsThisCombat;
        result.minCardsPlayedThisTurn = kiddo.minCardsPlayedThisTurn ?? man.minCardsPlayedThisTurn;
        result.minCombatsThisRun = kiddo.minCombatsThisRun ?? man.minCombatsThisRun;
        result.minCostOfCardJustPlayed = kiddo.minCostOfCardJustPlayed ?? man.minCostOfCardJustPlayed;
        result.minDamageBlockedByEnemyArmorThisTurn = kiddo.minDamageBlockedByEnemyArmorThisTurn ?? man.minDamageBlockedByEnemyArmorThisTurn;
        result.minDamageBlockedByPlayerArmorThisTurn = kiddo.minDamageBlockedByPlayerArmorThisTurn ?? man.minDamageBlockedByPlayerArmorThisTurn;
        result.minDamageDealtToEnemyThisAction = kiddo.minDamageDealtToEnemyThisAction ?? man.minDamageDealtToEnemyThisAction;
        result.minDamageDealtToEnemyThisTurn = kiddo.minDamageDealtToEnemyThisTurn ?? man.minDamageDealtToEnemyThisTurn;
        result.minDamageDealtToPlayerThisTurn = kiddo.minDamageDealtToPlayerThisTurn ?? man.minDamageDealtToPlayerThisTurn;
        result.minEnergy = kiddo.minEnergy ?? man.minEnergy;
        result.minHull = kiddo.minHull ?? man.minHull;
        result.minHullPercent = kiddo.minHullPercent ?? man.minHullPercent;
        result.minMovesThisTurn = kiddo.minMovesThisTurn ?? man.minMovesThisTurn;
        result.minRuns = kiddo.minRuns ?? man.minRuns;
        result.minTimesYouFlippedACardThisTurn = kiddo.minTimesYouFlippedACardThisTurn ?? man.minTimesYouFlippedACardThisTurn;
        result.minTurnsThisCombat = kiddo.minTurnsThisCombat ?? man.minTurnsThisCombat;
        result.minWinCount = kiddo.minWinCount ?? man.minWinCount;
        result.never = kiddo.never ?? man.never;
        result.nonePresent = kiddo.nonePresent ?? man.nonePresent;  //
        result.once = original.once == kiddo.once ? man.once : kiddo.once;
        result.oncePerCombat = original.oncePerCombat == kiddo.oncePerCombat ? man.oncePerCombat : kiddo.oncePerCombat;
        result.oncePerCombatTags = kiddo.oncePerCombatTags ?? man.oncePerCombatTags;  //
        result.oncePerRun = original.oncePerRun == kiddo.oncePerRun ? man.oncePerRun : kiddo.oncePerRun;
        result.oncePerRunTags = kiddo.oncePerRunTags ?? man.oncePerRunTags;  //
        result.pax = kiddo.pax ?? man.pax;
        result.playerJustPiercedEnemyArmor = kiddo.playerJustPiercedEnemyArmor ?? man.playerJustPiercedEnemyArmor;
        result.playerJustShotAMidrowObject = kiddo.playerJustShotAMidrowObject ?? man.playerJustShotAMidrowObject;
        result.playerJustShotASoccerBall = kiddo.playerJustShotASoccerBall ?? man.playerJustShotASoccerBall;
        result.playerJustShuffledDiscardIntoDrawPile = kiddo.playerJustShuffledDiscardIntoDrawPile ?? man.playerJustShuffledDiscardIntoDrawPile;
        result.playerShotJustHit = kiddo.playerShotJustHit ?? man.playerShotJustHit;
        result.playerShotJustMissed = kiddo.playerShotJustMissed ?? man.playerShotJustMissed;
        result.playerShotWasFromPayback = kiddo.playerShotWasFromPayback ?? man.playerShotWasFromPayback;
        result.playerShotWasFromStrafe = kiddo.playerShotWasFromStrafe ?? man.playerShotWasFromStrafe;
        result.priority = original.priority == kiddo.priority ? man.priority : kiddo.priority;
        result.requireCharsLocked = kiddo.requireCharsLocked ?? man.requireCharsLocked;  //
        result.requireCharsUnlocked = kiddo.requireCharsUnlocked ?? man.requireCharsUnlocked;  //
        result.requiredScenes = kiddo.requiredScenes ?? man.requiredScenes;  //
        result.shipsDontOverlapAtAll = kiddo.shipsDontOverlapAtAll ?? man.shipsDontOverlapAtAll;
        result.specialFight = kiddo.specialFight ?? man.specialFight;
        result.spikeName = kiddo.spikeName ?? man.spikeName;
        result.turnStart = kiddo.turnStart ?? man.turnStart;
        result.type = original.type == kiddo.type ? man.type : kiddo.type;
        result.wasGoingToOverheatButStopped = kiddo.wasGoingToOverheatButStopped ?? man.wasGoingToOverheatButStopped;
        result.whoDidThat = kiddo.whoDidThat ?? man.whoDidThat;
        result.zones = kiddo.zones ?? man.zones;  //

        if (appendLists)
        {
            dontAppendFields ??= [];
            if (!(dontAppendFields.Contains("allPresent") || excludeFields.Contains("allPresent"))) result.allPresent = [.. man.allPresent ?? [], .. kiddo.allPresent ?? []];
            if (!(dontAppendFields.Contains("anyDrones") || excludeFields.Contains("anyDrones"))) result.anyDrones = [.. man.anyDrones ?? [], .. kiddo.anyDrones ?? []];
            if (!(dontAppendFields.Contains("anyDronesFriendly") || excludeFields.Contains("anyDronesFriendly"))) result.anyDronesFriendly = [.. man.anyDronesFriendly ?? [], .. kiddo.anyDronesFriendly ?? []];
            if (!(dontAppendFields.Contains("anyDronesHostile") || excludeFields.Contains("anyDronesHostile"))) result.anyDronesHostile = [.. man.anyDronesHostile ?? [], .. kiddo.anyDronesHostile ?? []];
            if (!(dontAppendFields.Contains("bgSetup") || excludeFields.Contains("bgSetup"))) result.bgSetup = [.. man.bgSetup ?? [], .. kiddo.bgSetup ?? []];
            if (!(dontAppendFields.Contains("doesNotHaveArtifacts") || excludeFields.Contains("doesNotHaveArtifacts"))) result.doesNotHaveArtifacts = [.. man.doesNotHaveArtifacts ?? [], .. kiddo.doesNotHaveArtifacts ?? []];
            if (!(dontAppendFields.Contains("excludedScenes") || excludeFields.Contains("excludedScenes"))) result.excludedScenes = [.. man.excludedScenes ?? [], .. kiddo.excludedScenes ?? []];
            if (!(dontAppendFields.Contains("hasArtifacts") || excludeFields.Contains("hasArtifacts"))) result.hasArtifacts = [.. man.hasArtifacts ?? [], .. kiddo.hasArtifacts ?? []];
            if (!(dontAppendFields.Contains("lastTurnEnemyStatuses") || excludeFields.Contains("lastTurnEnemyStatuses"))) result.lastTurnEnemyStatuses = [.. man.lastTurnEnemyStatuses ?? [], .. kiddo.lastTurnEnemyStatuses ?? []];
            if (!(dontAppendFields.Contains("lastTurnPlayerStatuses") || excludeFields.Contains("lastTurnPlayerStatuses"))) result.lastTurnPlayerStatuses = [.. man.lastTurnPlayerStatuses ?? [], .. kiddo.lastTurnPlayerStatuses ?? []];
            if (!(dontAppendFields.Contains("lines") || excludeFields.Contains("lines"))) result.lines = [.. man.lines ?? [], .. kiddo.lines ?? []];
            if (!(dontAppendFields.Contains("lookup") || excludeFields.Contains("lookup"))) result.lookup = [.. man.lookup ?? [], .. kiddo.lookup ?? []];
            if (!(dontAppendFields.Contains("nonePresent") || excludeFields.Contains("nonePresent"))) result.nonePresent = [.. man.nonePresent ?? [], .. kiddo.nonePresent ?? []];
            if (!(dontAppendFields.Contains("oncePerCombatTags") || excludeFields.Contains("oncePerCombatTags"))) result.oncePerCombatTags = [.. man.oncePerCombatTags ?? [], .. kiddo.oncePerCombatTags ?? []];
            if (!(dontAppendFields.Contains("oncePerRunTags") || excludeFields.Contains("oncePerRunTags"))) result.oncePerRunTags = [.. man.oncePerRunTags ?? [], .. kiddo.oncePerRunTags ?? []];
            if (!(dontAppendFields.Contains("requireCharsLocked") || excludeFields.Contains("requireCharsLocked"))) result.requireCharsLocked = [.. man.requireCharsLocked ?? [], .. kiddo.requireCharsLocked ?? []];
            if (!(dontAppendFields.Contains("requireCharsUnlocked") || excludeFields.Contains("requireCharsUnlocked"))) result.requireCharsUnlocked = [.. man.requireCharsUnlocked ?? [], .. kiddo.requireCharsUnlocked ?? []];
            if (!(dontAppendFields.Contains("requiredScenes") || excludeFields.Contains("requiredScenes"))) result.requiredScenes = [.. man.requiredScenes ?? [], .. kiddo.requiredScenes ?? []];
            if (!(dontAppendFields.Contains("zones") || excludeFields.Contains("zones"))) result.zones = [.. man.zones ?? [], .. kiddo.zones ?? []];
        }

        return result;
    }

    /// <summary>
    /// Copies a DialogueMachine as a new copy
    /// </summary>
    /// <param name="origin">DialogueMachine to copy</param>
    /// <returns>New DialogueMachine with copied fields</returns>
    public static DialogueMachine NodeCopier(in DialogueMachine origin)
    {
        DialogueMachine? result = NodeCopier((StoryNode)origin) as DialogueMachine;
        if (result is not null)
        {
            result.edit = origin.edit?.Select(e => new EditThing(e)).ToList();
            result.dialogue = origin.dialogue?.Select(d => new DialogueThing(d)).ToList();
            result.hasArtifactTypes = origin.hasArtifactTypes?.ToHashSet();
            result.doesNotHaveArtifactTypes = origin.doesNotHaveArtifactTypes?.ToHashSet();
            result.dontAppendListFields = origin.dontAppendListFields?.ToList();
        }
        return result ?? new();
    }

    /// <summary>
    /// Copies a DialogueMachine as a new copy, excluding specified fields
    /// </summary>
    /// <param name="origin">DialogueMachine to copy</param>
    /// <param name="excludeFields">Fields to skip</param>
    /// <returns>New DialogueMachine with copied fields</returns>
    public static DialogueMachine NodeCopier(in DialogueMachine origin, params string[] excludeFields)
    {  // ERROR TYPECAST HERE
        DialogueMachine? result = NodeCopier((StoryNode)origin, excludeFields) as DialogueMachine;  // It typecasts from DialogueMachine to StoryNode just fine, verified using debugging, setting the breakpoint on this line (before I changed it to an explicit cast, it was NodeCopier() as DialogueMachine... but that always set result = null) and on StoryNode overload NodeCopier's return line.
        if (result is not null)
        {
            result.edit = excludeFields.Contains("edit") ? default : origin.edit?.Select(e => new EditThing(e)).ToList();
            result.dialogue = excludeFields.Contains("dialogue") ? default : origin.dialogue?.Select(d => new DialogueThing(d)).ToList();
            result.hasArtifactTypes = excludeFields.Contains("hasArtifactTypes") ? default : origin.hasArtifactTypes?.ToHashSet();
            result.doesNotHaveArtifactTypes = excludeFields.Contains("doesNotHaveArtifactTypes") ? default : origin.doesNotHaveArtifactTypes?.ToHashSet();
            result.dontAppendListFields = excludeFields.Contains("replaceFields") ? default : origin.dontAppendListFields?.ToList();
        }
        return result ?? new();
    }

    /// <summary>
    /// Copies a StoryNode as a new copy
    /// </summary>
    /// <param name="origin">StoryNode to copy</param>
    /// <returns>New StoryNode with copied fields</returns>
    public static StoryNode NodeCopier(in StoryNode origin)
    {
        StoryNode result = new DialogueMachine()
        {
            allPresent = origin.allPresent?.ToHashSet(),
            anyDrones = origin.anyDrones?.ToHashSet(),
            anyDronesFriendly = origin.anyDronesFriendly?.ToHashSet(),
            anyDronesHostile = origin.anyDronesHostile?.ToHashSet(),
            bg = origin.bg,
            bgSetup = origin.bgSetup?.ToList(),
            canSpawnOnMap = origin.canSpawnOnMap,
            choiceFunc = origin.choiceFunc,
            choiceText = origin.choiceText,
            demo = origin.demo,
            doesNotHaveArtifacts = origin.doesNotHaveArtifacts?.ToHashSet(),
            dontCountForProgression = origin.dontCountForProgression,
            enemyDoesNotHavePart = origin.enemyDoesNotHavePart,
            enemyHasArmoredPart = origin.enemyHasArmoredPart,
            enemyHasBrittlePart = origin.enemyHasBrittlePart,
            enemyHasPart = origin.enemyHasPart,
            enemyHasWeakPart = origin.enemyHasWeakPart,
            enemyIntent = origin.enemyIntent,
            enemyShotJustHit = origin.enemyShotJustHit,
            enemyShotJustMissed = origin.enemyShotJustMissed,
            excludedScenes = origin.excludedScenes.ToHashSet(),
            goingToOverheat = origin.goingToOverheat,
            handEmpty = origin.handEmpty,
            handFullOfTrash = origin.handFullOfTrash,
            handFullOfUnplayableCards = origin.handFullOfUnplayableCards,
            hasArtifacts = origin.hasArtifacts?.ToHashSet(),
            introDelay = origin.introDelay,
            justOverheated = origin.justOverheated,
            lastDeathZone = origin.lastDeathZone,
            lastNamedDroneDestroyed = origin.lastNamedDroneDestroyed,
            lastNamedDroneSpawned = origin.lastNamedDroneSpawned,
            lastTurnEnemyStatuses = origin.lastTurnEnemyStatuses?.ToHashSet(),
            lastTurnPlayerStatuses = origin.lastTurnPlayerStatuses?.ToHashSet(),
            lines = Mutil.DeepCopy(origin.lines),
            lookup = origin.lookup?.ToHashSet(),
            maxCostOfCardJustPlayed = origin.maxCostOfCardJustPlayed,
            maxDamageBlockedByEnemyArmorThisTurn = origin.maxDamageBlockedByEnemyArmorThisTurn,
            maxDamageDealtToEnemyThisAction = origin.maxDamageDealtToEnemyThisAction,
            maxDamageDealtToPlayerThisTurn = origin.maxDamageDealtToPlayerThisTurn,
            maxHull = origin.maxHull,
            maxHullPercent = origin.maxHullPercent,
            maxTurnsThisCombat = origin.maxTurnsThisCombat,
            minCardsPlayedThisTurn = origin.minCardsPlayedThisTurn,
            minCombatsThisRun = origin.minCombatsThisRun,
            minCostOfCardJustPlayed = origin.minCostOfCardJustPlayed,
            minDamageBlockedByEnemyArmorThisTurn = origin.minDamageBlockedByEnemyArmorThisTurn,
            minDamageBlockedByPlayerArmorThisTurn = origin.minDamageBlockedByPlayerArmorThisTurn,
            minDamageDealtToEnemyThisAction = origin.minDamageDealtToEnemyThisAction,
            minDamageDealtToEnemyThisTurn = origin.minDamageDealtToEnemyThisTurn,
            minDamageDealtToPlayerThisTurn = origin.minDamageDealtToPlayerThisTurn,
            minEnergy = origin.minEnergy,
            minHull = origin.minHull,
            minHullPercent = origin.minHullPercent,
            minMovesThisTurn = origin.minMovesThisTurn,
            minRuns = origin.minRuns,
            minTimesYouFlippedACardThisTurn = origin.minTimesYouFlippedACardThisTurn,
            minTurnsThisCombat = origin.minTurnsThisCombat,
            minWinCount = origin.minWinCount,
            never = origin.never,
            nonePresent = origin.nonePresent?.ToHashSet(),
            once = origin.once,
            oncePerCombat = origin.oncePerCombat,
            oncePerCombatTags = origin.oncePerCombatTags?.ToHashSet(),
            oncePerRun = origin.oncePerRun,
            oncePerRunTags = origin.oncePerRunTags?.ToHashSet(),
            pax = origin.pax,
            playerJustPiercedEnemyArmor = origin.playerJustPiercedEnemyArmor,
            playerJustShotAMidrowObject = origin.playerJustShotAMidrowObject,
            playerJustShotASoccerBall = origin.playerJustShotASoccerBall,
            playerJustShuffledDiscardIntoDrawPile = origin.playerJustShuffledDiscardIntoDrawPile,
            playerShotJustHit = origin.playerShotJustHit,
            playerShotJustMissed = origin.playerShotJustMissed,
            playerShotWasFromPayback = origin.playerShotWasFromPayback,
            playerShotWasFromStrafe = origin.playerShotWasFromStrafe,
            priority = origin.priority,
            requireCharsLocked = origin.requireCharsLocked?.ToHashSet(),
            requireCharsUnlocked = origin.requireCharsUnlocked?.ToHashSet(),
            requiredScenes = origin.requiredScenes.ToHashSet(),
            shipsDontOverlapAtAll = origin.shipsDontOverlapAtAll,
            specialFight = origin.specialFight,
            spikeName = origin.spikeName,
            turnStart = origin.turnStart,
            type = origin.type,
            wasGoingToOverheatButStopped = origin.wasGoingToOverheatButStopped,
            whoDidThat = origin.whoDidThat,
            zones = origin.zones?.ToHashSet()
        };
        return result;
    }

    /// <summary>
    /// Copies a StoryNode as a new copy, excluding specified fields
    /// </summary>
    /// <param name="origin">StoryNode to copy</param>
    /// <param name="excludeFields">Fields to skip</param>
    /// <returns>New StoryNode with copied fields</returns>
    public static StoryNode NodeCopier(in StoryNode origin, params string[] excludeFields)
    {
        StoryNode result = new DialogueMachine()
        {
            allPresent = excludeFields.Contains("allPresent") ? default : origin.allPresent?.ToHashSet(),
            anyDrones = excludeFields.Contains("anyDrones") ? default : origin.anyDrones?.ToHashSet(),
            anyDronesFriendly = excludeFields.Contains("anyDronesFriendly") ? default : origin.anyDronesFriendly?.ToHashSet(),
            anyDronesHostile = excludeFields.Contains("anyDronesHostile") ? default : origin.anyDronesHostile?.ToHashSet(),
            bg = excludeFields.Contains("bg") ? default : origin.bg,
            bgSetup = excludeFields.Contains("bgSetup") ? default : origin.bgSetup?.ToList(),
            canSpawnOnMap = excludeFields.Contains("canSpawnOnMap") ? default : origin.canSpawnOnMap,
            choiceFunc = excludeFields.Contains("choiceFunc") ? default : origin.choiceFunc,
            choiceText = excludeFields.Contains("choiceText") ? default : origin.choiceText,
            demo = excludeFields.Contains("demo") ? default : origin.demo,
            doesNotHaveArtifacts = excludeFields.Contains("doesNotHaveArtifacts") ? default : origin.doesNotHaveArtifacts?.ToHashSet(),
            dontCountForProgression = !excludeFields.Contains("dontCountForProgression") && origin.dontCountForProgression,
            enemyDoesNotHavePart = excludeFields.Contains("enemyDoesNotHavePart") ? default : origin.enemyDoesNotHavePart,
            enemyHasArmoredPart = excludeFields.Contains("enemyHasArmoredPart") ? default : origin.enemyHasArmoredPart,
            enemyHasBrittlePart = excludeFields.Contains("enemyHasBrittlePart") ? default : origin.enemyHasBrittlePart,
            enemyHasPart = excludeFields.Contains("enemyHasPart") ? default : origin.enemyHasPart,
            enemyHasWeakPart = excludeFields.Contains("enemyHasWeakPart") ? default : origin.enemyHasWeakPart,
            enemyIntent = excludeFields.Contains("enemyIntent") ? default : origin.enemyIntent,
            enemyShotJustHit = excludeFields.Contains("enemyShotJustHit") ? default : origin.enemyShotJustHit,
            enemyShotJustMissed = excludeFields.Contains("enemyShotJustMissed") ? default : origin.enemyShotJustMissed,
            excludedScenes = excludeFields.Contains("excludedScenes") ? [] : origin.excludedScenes.ToHashSet(),
            goingToOverheat = excludeFields.Contains("goingToOverheat") ? default : origin.goingToOverheat,
            handEmpty = excludeFields.Contains("handEmpty") ? default : origin.handEmpty,
            handFullOfTrash = excludeFields.Contains("handFullOfTrash") ? default : origin.handFullOfTrash,
            handFullOfUnplayableCards = excludeFields.Contains("handFullOfUnplayableCards") ? default : origin.handFullOfUnplayableCards,
            hasArtifacts = excludeFields.Contains("hasArtifacts") ? default : origin.hasArtifacts?.ToHashSet(),
            introDelay = excludeFields.Contains("introDelay") ? default : origin.introDelay,
            justOverheated = excludeFields.Contains("justOverheated") ? default : origin.justOverheated,
            lastDeathZone = excludeFields.Contains("lastDeathZone") ? default : origin.lastDeathZone,
            lastNamedDroneDestroyed = excludeFields.Contains("lastNamedDroneDestroyed") ? default : origin.lastNamedDroneDestroyed,
            lastNamedDroneSpawned = excludeFields.Contains("lastNamedDroneSpawned") ? default : origin.lastNamedDroneSpawned,
            lastTurnEnemyStatuses = excludeFields.Contains("lastTurnEnemyStatuses") ? default : origin.lastTurnEnemyStatuses?.ToHashSet(),
            lastTurnPlayerStatuses = excludeFields.Contains("lastTurnPlayerStatuses") ? default : origin.lastTurnPlayerStatuses?.ToHashSet(),
            lines = excludeFields.Contains("lines") ? [] : Mutil.DeepCopy(origin.lines),
            lookup = excludeFields.Contains("lookup") ? default : origin.lookup?.ToHashSet(),
            maxCostOfCardJustPlayed = excludeFields.Contains("maxCostOfCardJustPlayed") ? default : origin.maxCostOfCardJustPlayed,
            maxDamageBlockedByEnemyArmorThisTurn = excludeFields.Contains("maxDamageBlockedByEnemyArmorThisTurn") ? default : origin.maxDamageBlockedByEnemyArmorThisTurn,
            maxDamageDealtToEnemyThisAction = excludeFields.Contains("maxDamageDealtToEnemyThisAction") ? default : origin.maxDamageDealtToEnemyThisAction,
            maxDamageDealtToPlayerThisTurn = excludeFields.Contains("maxDamageDealtToPlayerThisTurn") ? default : origin.maxDamageDealtToPlayerThisTurn,
            maxHull = excludeFields.Contains("maxHull") ? default : origin.maxHull,
            maxHullPercent = excludeFields.Contains("maxHullPercent") ? default : origin.maxHullPercent,
            maxTurnsThisCombat = excludeFields.Contains("maxTurnsThisCombat") ? default : origin.maxTurnsThisCombat,
            minCardsPlayedThisTurn = excludeFields.Contains("minCardsPlayedThisTurn") ? default : origin.minCardsPlayedThisTurn,
            minCombatsThisRun = excludeFields.Contains("minCombatsThisRun") ? default : origin.minCombatsThisRun,
            minCostOfCardJustPlayed = excludeFields.Contains("minCostOfCardJustPlayed") ? default : origin.minCostOfCardJustPlayed,
            minDamageBlockedByEnemyArmorThisTurn = excludeFields.Contains("minDamageBlockedByEnemyArmorThisTurn") ? default : origin.minDamageBlockedByEnemyArmorThisTurn,
            minDamageBlockedByPlayerArmorThisTurn = excludeFields.Contains("minDamageBlockedByPlayerArmorThisTurn") ? default : origin.minDamageBlockedByPlayerArmorThisTurn,
            minDamageDealtToEnemyThisAction = excludeFields.Contains("minDamageDealtToEnemyThisAction") ? default : origin.minDamageDealtToEnemyThisAction,
            minDamageDealtToEnemyThisTurn = excludeFields.Contains("minDamageDealtToEnemyThisTurn") ? default : origin.minDamageDealtToEnemyThisTurn,
            minDamageDealtToPlayerThisTurn = excludeFields.Contains("minDamageDealtToPlayerThisTurn") ? default : origin.minDamageDealtToPlayerThisTurn,
            minEnergy = excludeFields.Contains("minEnergy") ? default : origin.minEnergy,
            minHull = excludeFields.Contains("minHull") ? default : origin.minHull,
            minHullPercent = excludeFields.Contains("minHullPercent") ? default : origin.minHullPercent,
            minMovesThisTurn = excludeFields.Contains("minMovesThisTurn") ? default : origin.minMovesThisTurn,
            minRuns = excludeFields.Contains("minRuns") ? default : origin.minRuns,
            minTimesYouFlippedACardThisTurn = excludeFields.Contains("minTimesYouFlippedACardThisTurn") ? default : origin.minTimesYouFlippedACardThisTurn,
            minTurnsThisCombat = excludeFields.Contains("minTurnsThisCombat") ? default : origin.minTurnsThisCombat,
            minWinCount = excludeFields.Contains("minWinCount") ? default : origin.minWinCount,
            never = excludeFields.Contains("never") ? default : origin.never,
            nonePresent = excludeFields.Contains("nonePresent") ? default : origin.nonePresent?.ToHashSet(),
            once = !excludeFields.Contains("once") && origin.once,
            oncePerCombat = !excludeFields.Contains("oncePerCombat") && origin.oncePerCombat,
            oncePerCombatTags = excludeFields.Contains("oncePerCombatTags") ? default : origin.oncePerCombatTags?.ToHashSet(),
            oncePerRun = !excludeFields.Contains("oncePerRun") && origin.oncePerRun,
            oncePerRunTags = excludeFields.Contains("oncePerRunTags") ? default : origin.oncePerRunTags?.ToHashSet(),
            pax = excludeFields.Contains("pax") ? default : origin.pax,
            playerJustPiercedEnemyArmor = excludeFields.Contains("playerJustPiercedEnemyArmor") ? default : origin.playerJustPiercedEnemyArmor,
            playerJustShotAMidrowObject = excludeFields.Contains("playerJustShotAMidrowObject") ? default : origin.playerJustShotAMidrowObject,
            playerJustShotASoccerBall = excludeFields.Contains("playerJustShotASoccerBall") ? default : origin.playerJustShotASoccerBall,
            playerJustShuffledDiscardIntoDrawPile = excludeFields.Contains("playerJustShuffledDiscardIntoDrawPile") ? default : origin.playerJustShuffledDiscardIntoDrawPile,
            playerShotJustHit = excludeFields.Contains("playerShotJustHit") ? default : origin.playerShotJustHit,
            playerShotJustMissed = excludeFields.Contains("playerShotJustMissed") ? default : origin.playerShotJustMissed,
            playerShotWasFromPayback = excludeFields.Contains("playerShotWasFromPayback") ? default : origin.playerShotWasFromPayback,
            playerShotWasFromStrafe = excludeFields.Contains("playerShotWasFromStrafe") ? default : origin.playerShotWasFromStrafe,
            priority = !excludeFields.Contains("priority") && origin.priority,
            requireCharsLocked = excludeFields.Contains("requireCharsLocked") ? default : origin.requireCharsLocked?.ToHashSet(),
            requireCharsUnlocked = excludeFields.Contains("requireCharsUnlocked") ? default : origin.requireCharsUnlocked?.ToHashSet(),
            requiredScenes = excludeFields.Contains("requiredScenes") ? [] : origin.requiredScenes.ToHashSet(),
            shipsDontOverlapAtAll = excludeFields.Contains("shipsDontOverlapAtAll") ? default : origin.shipsDontOverlapAtAll,
            specialFight = excludeFields.Contains("specialFight") ? default : origin.specialFight,
            spikeName = excludeFields.Contains("spikeName") ? default : origin.spikeName,
            turnStart = excludeFields.Contains("turnStart") ? default : origin.turnStart,
            type = excludeFields.Contains("type") ? default : origin.type,
            wasGoingToOverheatButStopped = excludeFields.Contains("wasGoingToOverheatButStopped") ? default : origin.wasGoingToOverheatButStopped,
            whoDidThat = excludeFields.Contains("whoDidThat") ? default : origin.whoDidThat,
            zones = excludeFields.Contains("zones") ? default : origin.zones?.ToHashSet()
        };
        return result;
    }
}