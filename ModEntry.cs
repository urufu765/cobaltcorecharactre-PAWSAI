using HarmonyLib;
using Microsoft.Extensions.Logging;
using Nanoray.PluginManager;
using Nickel;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JollyJolly.Artifacts;
// using JollyJolly.Cards;
using JollyJolly.External;
// using JollyJolly.Features;
using System.Reflection;
// using JollyJolly.Actions;
using JollyJolly.Conversation;
//using System.Reflection;

namespace JollyJolly;

internal partial class ModEntry : SimpleMod
{
    public ModEntry(IPluginPackage<IModManifest> package, IModHelper helper, ILogger logger) : base(package, helper, logger)
    {
        Instance = this;
        Harmony = new Harmony("urufudoggo.JollyJolly");
        UniqueName = package.Manifest.UniqueName;
        modDialogueInited = false;
        /*
         * Some mods provide an API, which can be requested from the ModRegistry.
         * The following is an example of a required dependency - the code would have unexpected errors if Kokoro was not present.
         * Dependencies can (and should) be defined within the nickel.json file, to ensure proper load mod load order.
         */
        KokoroApi = helper.ModRegistry.GetApi<IKokoroApi>("Shockah.Kokoro")!;
        MoreDifficultiesApi = helper.ModRegistry.GetApi<IMoreDifficultiesApi>("TheJazMaster.MoreDifficulties");
        DuoArtifactsApi = helper.ModRegistry.GetApi<IDuoArtifactsApi>("Shockah.DuoArtifacts");
        helper.Events.OnModLoadPhaseFinished += (_, phase) =>
        {
            if (phase == ModLoadPhase.AfterDbInit)
            {
                if (DuoArtifactsApi is not null)
                {
                    foreach (Type type in WethDuoArtifacts)
                    {
                        DuoArtifactMeta? dam = type.GetCustomAttribute<DuoArtifactMeta>();
                        if (dam is not null)
                        {
                            if (dam.duoModDeck is null)
                            {
                                DuoArtifactsApi.RegisterDuoArtifact(type, [WethDeck!.Deck, dam.duoDeck]);
                            }
                            else if (helper.Content.Decks.LookupByUniqueName(dam.duoModDeck) is IDeckEntry ide)
                            {
                                DuoArtifactsApi.RegisterDuoArtifact(type, [WethDeck!.Deck, ide.Deck]);
                            }
                        }
                    }
                }
                foreach (Type type in WethDialogues)
                    AccessTools.DeclaredMethod(type, nameof(IDialogueRegisterable.LateRegister))?.Invoke(null, null);
                localDB = new(helper, package);
            }
        };
        helper.Events.OnLoadStringsForLocale += (_, thing) =>
        {
            foreach (KeyValuePair<string, string> entry in localDB.GetLocalizationResults())
            {
                thing.Localizations[entry.Key] = entry.Value;
            }
        };

        AnyLocalizations = new JsonLocalizationProvider(
            tokenExtractor: new SimpleLocalizationTokenExtractor(),
            localeStreamFunction: locale => package.PackageRoot.GetRelativeFile($"i18n/{locale}.json").OpenRead()
        );
        Localizations = new MissingPlaceholderLocalizationProvider<IReadOnlyList<string>>(
            new CurrentLocaleOrEnglishLocalizationProvider<IReadOnlyList<string>>(AnyLocalizations)
        );

        /*
         * A deck only defines how cards should be grouped, for things such as codex sorting and Second Opinions.
         * A character must be defined with a deck to allow the cards to be obtainable as a character's cards.
         */
        WethDeck = helper.Content.Decks.RegisterDeck("weth", new DeckConfiguration
        {
            Definition = new DeckDef
            {
                /*
                 * This color is used in a few places:
                 * TODO On cards, it dictates the sheen on higher rarities, as well as influences the color of the energy cost.
                 * If this deck is given to a playable character, their name will be this color, and their mini will have this color as their border.
                 */
                color = new Color("2a767d"),

                titleColor = new Color("93c4c8").addClarityBright()
            },

            DefaultCardArt = StableSpr.cards_Cannon,
            BorderSprite = RegisterSprite(package, "assets/Borders/frame_weth.png").Sprite,
            Name = AnyLocalizations.Bind(["character", "Weth", "name"]).Localize,
            ShineColorOverride = _ => new Color(0, 0, 0),
        });

        /*
         * Characters have required animations, recommended animations, and you have the option to add more.
         * In addition, they must be registered before the character themselves is registered.
         * The game requires you to have a neutral animation and mini animation, used for normal gameplay and the map and run start screen, respectively.
         * The game uses the squint animation for the Extra-Planar Being and High-Pitched Static events, and the gameover animation while you are dying.
         * You may define any other animations, and they will only be used when explicitly referenced (such as dialogue).
         */
        foreach (string s1 in Weth1Anims)
        {
            RegisterAnimation(package, s1, $"assets/Animation/weth_{s1}", 1);
        }
        foreach (string s3 in Weth3Anims)
        {
            RegisterAnimation(package, s3, $"assets/Animation/weth_{s3}", 3);
        }
        foreach (string s4 in Weth4Anims)
        {
            RegisterAnimation(package, s4, $"assets/Animation/weth_{s4}", 4);
        }
        foreach (string s5 in Weth5Anims)
        {
            RegisterAnimation(package, s5, $"assets/Animation/weth_{s5}", 5);
        }
        foreach (string s6 in Weth6Anims)
        {
            RegisterAnimation(package, s6, $"assets/Animation/weth_{s6}", 6);
        }

        WethTheSnep = helper.Content.Characters.V2.RegisterPlayableCharacter("weth", new PlayableCharacterConfigurationV2
        {
            Deck = WethDeck.Deck,
            BorderSprite = WethFramePast,
            Starters = new StarterDeck
            {
                cards = [
                    new TrashDispenser(),
                    new SplitshotCard()
                ],
                /*
                 * Some characters have starting artifacts, in addition to starting cards.
                 * This is where they would be added, much like their starter cards.
                 * This can be safely removed if you have no starting artifacts.
                 */
                artifacts = [
                    new TreasureHunter()
                ]
            },
            Description = AnyLocalizations.Bind(["character", "Weth", "desc"]).Localize,
            SoloStarters = new StarterDeck
            {
                cards = [
                    new Bloom(),
                    new DodgeColorless(),
                    new TrashDispenser(),
                    new GiantTrash(),
                    new SplitshotCard(),
                    new SplitshotCard()
                ],
                artifacts = [
                    new TreasureHunter()
                ]
            },
            ExeCardType = typeof(WethExe)
        });

        MoreDifficultiesApi?.RegisterAltStarters(WethDeck.Deck, new StarterDeck
        {
            cards = [
                new PulsedriveCard(),
                new Puckshot(),
            ],
            artifacts =
            [
                new TreasureHunter()
            ]
        });

        /*
         * Statuses are used to achieve many mechanics.
         * However, statuses themselves do not contain any code - they just keep track of how much you have.
         */
        /*
         * Managers are typically made to register themselves when constructed.
         * _ = makes the compiler not complain about the fact that you are constructing something for seemingly no reason.
         */
        //_ = new KnowledgeManager();
        //_ = new Otherdriving();

        /*
         * Some classes require so little management that a manager may not be worth writing.
         * In AGainPonder's case, it is simply a need for two sprites and evaluation of an artifact's effect.
         */
        //AGainPonder.DrawSpr = RegisterSprite(package, "assets/ponder_draw.png").Sprite;
        //AGainPonder.DiscardSpr = RegisterSprite(package, "assets/ponder_discard.png").Sprite;
        //AOverthink.Spr = RegisterSprite(package, "assets/overthink.png").Sprite;

        // Artifact Section
        foreach (Type ta in WethArtifactTypes)
        {
            Deck deck = WethDeck.Deck;
            if (WethDuoArtifacts.Contains(ta))
            {
                if (DuoArtifactsApi is null)
                {
                    continue;
                }
                else
                {
                    deck = DuoArtifactsApi.DuoArtifactVanillaDeck;
                }
            }
            helper.Content.Artifacts.RegisterArtifact(ta.Name, UhDuhHundo.ArtifactRegistrationHelper(ta, RegisterSprite(package, "assets/Artifact/" + ta.Name + ".png").Sprite, deck));
        }

        /*
         * All the IRegisterable types placed into the static lists at the start of the class are initialized here.
         * This snippet invokes all of them, allowing them to register themselves with the package and helper.
         */
        foreach (var type in AllRegisterableTypes)
            AccessTools.DeclaredMethod(type, nameof(IRegisterable.Register))?.Invoke(null, [package, helper]);

        Apply(Harmony);
        //BayBlastIconography.Apply(Harmony);
    }

    /*
     * assets must also be registered before they may be used.
     * Unlike cards and artifacts, however, they are very simple to register, and often do not need to be referenced in more than one place.
     * This utility method exists to easily register a sprite, but nothing prevents you from calling the method used yourself.
     */
    public static ISpriteEntry RegisterSprite(IPluginPackage<IModManifest> package, string dir)
    {
        return Instance.Helper.Content.Sprites.RegisterSprite(package.PackageRoot.GetRelativeFile(dir));
    }

    public static ISoundEntry RegisterSound(IPluginPackage<IModManifest> package, string dir)
    {
        return Instance.Helper.Content.Audio.RegisterSound(package.PackageRoot.GetRelativeFile(dir));
    }

    /*
     * Animation frames are typically named very similarly, only differing by the number of the frame itself.
     * This utility method exists to easily register an animation.
     * It expects the animation to start at frame 0, up to frames - 1.
     * TODO It is advised to avoid animations consisting of 2 or 3 frames.
     */
    public static ICharacterAnimationEntryV2 RegisterAnimation(IPluginPackage<IModManifest> package, string tag, string dir, int frames)
    {
        return Instance.Helper.Content.Characters.V2.RegisterCharacterAnimation(new CharacterAnimationConfigurationV2
        {
            CharacterType = Instance.WethDeck.Deck.Key(),
            LoopTag = tag,
            Frames = Enumerable.Range(0, frames)
                .Select(i => RegisterSprite(package, dir + i + ".png").Sprite)
                .ToImmutableList()
        });
    }
}

