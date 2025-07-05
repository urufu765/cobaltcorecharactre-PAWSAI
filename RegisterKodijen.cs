using HarmonyLib;
using Nickel;
using Nanoray.PluginManager;
using System;
using System.Collections.Generic;

namespace Starhunters;

internal partial class ModEntry : SimpleMod
{
    public void Register_Kodijen(IPluginPackage<IModManifest> package, IModHelper helper)
    {
        KodijenDeck = helper.Content.Decks.RegisterDeck("kodijen", new DeckConfiguration
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
            BorderSprite = RegisterSprite(package, "assets/frame/frame_kodijen.png").Sprite,
            Name = AnyLocalizations.Bind(["Kodijen", "character", "name"]).Localize
        });

        foreach (KeyValuePair<int, List<string>> anims in KodijenAnims)
        {
            foreach (string anim in anims.Value)
            {
                RegisterAnimation(KodijenDeck.Deck.Key(), package, anim, $"assets/animation/kodijen/kodijen_{anim}", anims.Key);
            }
        }

        Kodijen = helper.Content.Characters.V2.RegisterPlayableCharacter("kodijen", new PlayableCharacterConfigurationV2
        {
            Deck = KodijenDeck.Deck,
            BorderSprite = RegisterSprite(package, "assets/frame/char_frame_kodijen.png").Sprite,
            Starters = new StarterDeck
            {
                cards = [
                    new SmartDrone(),
                    new DefensiveCommand()
                ],
                artifacts = [
                ]
            },
            Description = AnyLocalizations.Bind(["Kodijen", "character", "desc"]).Localize,
            ExeCardType = typeof(KodijenExe)
        });

        MoreDifficultiesApi?.RegisterAltStarters(KodijenDeck.Deck, new StarterDeck
        {
            cards = [
                new MissileDrone(),
                new ShiftyMissile()
            ],
            artifacts =
            [
            ]
        });

        Status_BorrowedHull = helper.Content.Statuses.RegisterStatus("BorrowedHull", new StatusConfiguration
        {
            Definition = new StatusDef
            {
                isGood = false,
                affectedByTimestop = false,
                color = new Color("00ff00"),
                icon = ModEntry.RegisterSprite(package, "assets/icon/kodijen/Status_BorrowedHull.png").Sprite
            },
            Name = AnyLocalizations.Bind(["Kodijen", "status", "BorrowedHull", "name"]).Localize,
            Description = AnyLocalizations.Bind(["Kodijen", "status", "BorrowedHull", "desc"]).Localize
        });
        Status_HullCapacity = helper.Content.Statuses.RegisterStatus("HullCapacity", new StatusConfiguration
        {
            Definition = new StatusDef
            {
                isGood = true,
                affectedByTimestop = false,
                color = new Color("00c8ff"),
                icon = ModEntry.RegisterSprite(package, "assets/icon/kodijen/Status_HullCapacity.png").Sprite
            },
            Name = AnyLocalizations.Bind(["Kodijen", "status", "HullCapacity", "name"]).Localize,
            Description = AnyLocalizations.Bind(["Kodijen", "status", "HullCapacity", "desc"]).Localize
        });

        foreach (Type ta in KodijenArtifactTypes)
        {
            Deck deck = KodijenDeck.Deck;
            if (KodijenDuoArtifactTypes.Contains(ta))
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
            helper.Content.Artifacts.RegisterArtifact(ta.Name, UhDuhHundo.ArtifactRegistrationHelper(ta, RegisterSprite(package, "assets/artifact/kodijen/" + ta.Name + ".png").Sprite, deck, "Kodijen"));
        }

        // _ = new ShieldRegen();
        // _ = new Repetition();
        // _ = new Repulsion();
    }
}