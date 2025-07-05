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
using Starhunters.Bruno.Cards;
using Starhunters.Bruno.Statuses;
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
                    foreach (Type type in BrunoDuoArtifactTypes)
                    {
                        DuoArtifactMeta? dam = type.GetCustomAttribute<DuoArtifactMeta>();
                        if (dam is not null)
                        {
                            if (dam.duoModDeck is null)
                            {
                                DuoArtifactsApi.RegisterDuoArtifact(type, [BrunoDeck!.Deck, dam.duoDeck]);
                            }
                            else if (helper.Content.Decks.LookupByUniqueName(dam.duoModDeck) is IDeckEntry ide)
                            {
                                DuoArtifactsApi.RegisterDuoArtifact(type, [BrunoDeck!.Deck, ide.Deck]);
                            }
                        }
                    }
                    foreach (Type type in KodijenDuoArtifactTypes)
                    {
                        DuoArtifactMeta? dam = type.GetCustomAttribute<DuoArtifactMeta>();
                        if (dam is not null)
                        {
                            if (dam.duoModDeck is null)
                            {
                                DuoArtifactsApi.RegisterDuoArtifact(type, [KodijenDeck!.Deck, dam.duoDeck]);
                            }
                            else if (helper.Content.Decks.LookupByUniqueName(dam.duoModDeck) is IDeckEntry ide)
                            {
                                DuoArtifactsApi.RegisterDuoArtifact(type, [KodijenDeck!.Deck, ide.Deck]);
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


        Register_Pawsai(package, helper);
        Register_Bruno(package, helper);
        Register_Kodijen(package, helper);
        // Register_Parmesan(package, helper);
        // Register_Varrigradona(package, helper);


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
                api.MakePadding(
                    api.MakeText(
                        () => Localizations.Localize(["Generic", "settings", "AccurateCalculation", "name"])
                    ).SetFont(DB.thicket), 8, 4
                ),
                api.MakeCheckbox(
                    () => Localizations.Localize(["Generic", "settings", "BetterEmergencyRetreat", "name"]),
                    () => settings.ProfileBased.Current.Pawsai_BetterEmergencyRetreat,
                    (_, _, value) => settings.ProfileBased.Current.Pawsai_BetterEmergencyRetreat = value
                ).SetTooltips(() => [
                    new GlossaryTooltip("starhuntersSettings.BetterEmergencyRetreat")
                    {
                        Description = Localizations.Localize(["Generic", "settings", "BetterEmergencyRetreat", "desc"])
                    }
                ]),
                api.MakeCheckbox(
                    () => Localizations.Localize(["Generic", "settings", "BetterHyperdrive", "name"]),
                    () => settings.ProfileBased.Current.Bruno_FancyHyperdrive,
                    (_, _, value) => settings.ProfileBased.Current.Bruno_FancyHyperdrive = value
                ).SetTooltips(() => [
                    new GlossaryTooltip("starhuntersSettings.BetterHyperdrive")
                    {
                        Description = Localizations.Localize(["Generic", "settings", "BetterHyperdrive", "desc"])
                    }
                ]),
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

