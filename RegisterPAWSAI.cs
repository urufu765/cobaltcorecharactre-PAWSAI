using HarmonyLib;
using Nickel;
using Nanoray.PluginManager;
using Starhunters.Pawsai.Statuses;
using System;
using Starhunters.Pawsai.Cards;
using System.Collections.Generic;

namespace Starhunters;

internal partial class ModEntry : SimpleMod
{
    public void Register_Pawsai(IPluginPackage<IModManifest> package, IModHelper helper)
    {
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

        Status_Regeneration = helper.Content.Statuses.RegisterStatus("ShieldRegeneration", new StatusConfiguration
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
        _ = new Repulsion();
    }
}