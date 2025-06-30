using HarmonyLib;
using Microsoft.Extensions.Logging;
using Nanoray.PluginManager;
using Nickel;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Starhunters.Artifacts;
// using JollyJolly.Cards;
using Starhunters.External;
// using JollyJolly.Features;
using System.Reflection;
// using JollyJolly.Actions;
using Starhunters.Conversation;
using Starhunters.Pawsai.Cards;
using Starhunters.Pawsai.Statuses;
//using System.Reflection;

namespace Starhunters;

internal partial class ModEntry : SimpleMod
{
    public ModEntry(IPluginPackage<IModManifest> package, IModHelper helper, ILogger logger) : base(package, helper, logger)
    {
        Instance = this;
        Harmony = new Harmony("urufudoggo.Starhunters");
        UniqueName = package.Manifest.UniqueName;
        /*
         * Some mods provide an API, which can be requested from the ModRegistry.
         * The following is an example of a required dependency - the code would have unexpected errors if Kokoro was not present.
         * Dependencies can (and should) be defined within the nickel.json file, to ensure proper load mod load order.
         */
        KokoroApi = helper.ModRegistry.GetApi<IKokoroApi>("Shockah.Kokoro")!;
        MoreDifficultiesApi = helper.ModRegistry.GetApi<IMoreDifficultiesApi>("TheJazMaster.MoreDifficulties");
        DuoArtifactsApi = helper.ModRegistry.GetApi<IDuoArtifactsApi>("Shockah.DuoArtifacts");
        settings = helper.Storage.LoadJson<Settings>(SettingsFile);

        helper.Events.OnModLoadPhaseFinished += (_, phase) =>
        {
            if (phase == ModLoadPhase.AfterDbInit)
            {
                if (DuoArtifactsApi is not null)
                {
                    foreach (Type type in PawsaiDuoArtifactTypes)
                    {
                        DuoArtifactMeta? dam = type.GetCustomAttribute<DuoArtifactMeta>();
                        if (dam is not null)
                        {
                            if (dam.duoModDeck is null)
                            {
                                DuoArtifactsApi.RegisterDuoArtifact(type, [PawsaiDeck!.Deck, dam.duoDeck]);
                            }
                            else if (helper.Content.Decks.LookupByUniqueName(dam.duoModDeck) is IDeckEntry ide)
                            {
                                DuoArtifactsApi.RegisterDuoArtifact(type, [PawsaiDeck!.Deck, ide.Deck]);
                            }
                        }
                    }
                }
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


        SpriteLoading.RegisterSprites(typeof(ModEntry), package);


        #region PAWSAI stuff
        PawsaiDeck = helper.Content.Decks.RegisterDeck("pawsai", new DeckConfiguration
        {
            Definition = new DeckDef
            {
                /*
                 * This color is used in a few places:
                 * TODO On cards, it dictates the sheen on higher rarities, as well as influences the color of the energy cost.
                 * If this deck is given to a playable character, their name will be this color, and their mini will have this color as their border.
                 */
                color = new Color("98a9ba"),

                titleColor = new Color("113f15")
            },

            DefaultCardArt = StableSpr.cards_Cannon,
            BorderSprite = RegisterSprite(package, "assets/frame/frame_pawsai.png").Sprite,
            Name = AnyLocalizations.Bind(["Pawsai", "character", "name"]).Localize
        });

        foreach (KeyValuePair<int, List<string>> anims in PawsaiAnims)
        {
            foreach (string anim in anims.Value)
            {
                RegisterAnimation(PawsaiDeck.Deck.Key(), package, anim, $"assets/animation/pawsai/pawsai_{anim}", anims.Key);
            }
        }

        Pawsai = helper.Content.Characters.V2.RegisterPlayableCharacter("pawsai", new PlayableCharacterConfigurationV2
        {
            Deck = PawsaiDeck.Deck,
            BorderSprite = RegisterSprite(package, "assets/frame/char_frame_pawsai.png").Sprite,
            Starters = new StarterDeck
            {
                cards = [
                    new PullShot(),
                    new Safeguard()
                ],
                artifacts = [
                ]
            },
            Description = AnyLocalizations.Bind(["Pawsai", "character", "desc"]).Localize,
            ExeCardType = typeof(PawsaiExe)
        });

        MoreDifficultiesApi?.RegisterAltStarters(PawsaiDeck.Deck, new StarterDeck
        {
            cards = [
                new RepulsionCard(),
                new ShieldRecovery()
            ],
            artifacts =
            [
            ]
        });

        Status_Regeneration = helper.Content.Statuses.RegisterStatus("Tarnish", new StatusConfiguration
        {
            Definition = new StatusDef
            {
                isGood = true,
                affectedByTimestop = true,
                color = new Color("00ff00"),
                icon = ModEntry.RegisterSprite(package, "assets/icon/pawsai/Status_Regeneration.png").Sprite
            },
            Name = AnyLocalizations.Bind(["Pawsai", "status", "Regeneration", "name"]).Localize,
            Description = AnyLocalizations.Bind(["Pawsai", "status", "Regeneration", "desc"]).Localize
        });
        Status_Repulsion = helper.Content.Statuses.RegisterStatus("Repulsion", new StatusConfiguration
        {
            Definition = new StatusDef
            {
                isGood = true,
                affectedByTimestop = true,
                color = new Color("00c8ff"),
                icon = ModEntry.RegisterSprite(package, "assets/icon/pawsai/Status_Repulsion.png").Sprite
            },
            Name = AnyLocalizations.Bind(["Pawsai", "status", "Repulsion", "name"]).Localize,
            Description = AnyLocalizations.Bind(["Pawsai", "status", "Repulsion", "desc"]).Localize
        });
        Status_Repetition = helper.Content.Statuses.RegisterStatus("Repetition", new StatusConfiguration
        {
            Definition = new StatusDef
            {
                isGood = true,
                affectedByTimestop = true,
                color = new Color("ff8500"),
                icon = ModEntry.RegisterSprite(package, "assets/icon/pawsai/Status_Repetition.png").Sprite
            },
            Name = AnyLocalizations.Bind(["Pawsai", "status", "Repetition", "name"]).Localize,
            Description = AnyLocalizations.Bind(["Pawsai", "status", "Repetition", "desc"]).Localize
        });

        foreach (Type ta in PawsaiArtifactTypes)
        {
            Deck deck = PawsaiDeck.Deck;
            if (PawsaiDuoArtifactTypes.Contains(ta))
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
            helper.Content.Artifacts.RegisterArtifact(ta.Name, UhDuhHundo.ArtifactRegistrationHelper(ta, RegisterSprite(package, "assets/artifact/pawsai/" + ta.Name + ".png").Sprite, deck, "Pawsai"));
        }

        _ = new ShieldRegen();
        _ = new Repetition();
        //_ = new Repulsion();

        #endregion


        foreach (var type in AllRegisterableTypes)
            AccessTools.DeclaredMethod(type, nameof(IRegisterable.Register))?.Invoke(null, [package, helper]);

        Apply(Harmony);

        helper.ModRegistry.AwaitApi<IModSettingsApi>(
            "Nickel.ModSettings",
            api => api.RegisterModSettings(api.MakeList([
                api.MakeProfileSelector(
                    () => package.Manifest.DisplayName ?? package.Manifest.UniqueName,
                    settings.ProfileBased
                ),
                api.MakeCheckbox(
                    () => Localizations.Localize(["Generic", "settings", "AccurateCalculation", "name"]),
                    () => settings.ProfileBased.Current.AccurateCalculations,
                    (_, _, value) => settings.ProfileBased.Current.AccurateCalculations = value
                ).SetTooltips(() => [
                    new GlossaryTooltip("starhuntersSettings.accurateCalculation")
                    {
                        Description = Localizations.Localize(["Generic", "settings", "AccurateCalculation", "desc"])
                    }
                ])
            ]).SubscribeToOnMenuClose(_ =>
            {
                helper.Storage.SaveJson(SettingsFile, settings);
            }))
        );
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
    public static ICharacterAnimationEntryV2 RegisterAnimation(string characterType, IPluginPackage<IModManifest> package, string tag, string dir, int frames)
    {
        return Instance.Helper.Content.Characters.V2.RegisterCharacterAnimation(new CharacterAnimationConfigurationV2
        {
            CharacterType = characterType,
            LoopTag = tag,
            Frames = Enumerable.Range(0, frames)
                .Select(i => RegisterSprite(package, dir + i + ".png").Sprite)
                .ToImmutableList()
        });
    }
}

