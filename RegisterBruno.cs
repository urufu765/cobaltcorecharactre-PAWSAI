using HarmonyLib;
using Nickel;
using Nanoray.PluginManager;
using Starhunters.Bruno.Statuses;
using System;
using Starhunters.Bruno.Cards;
using System.Collections.Generic;

namespace Starhunters;

internal partial class ModEntry : SimpleMod
{
    public void Register_Bruno(IPluginPackage<IModManifest> package, IModHelper helper)
    {
        BrunoDeck = helper.Content.Decks.RegisterDeck("bruno", new DeckConfiguration
        {
            Definition = new DeckDef
            {
                /*
                 * This color is used in a few places:
                 * TODO On cards, it dictates the sheen on higher rarities, as well as influences the color of the energy cost.
                 * If this deck is given to a playable character, their name will be this color, and their mini will have this color as their border.
                 */
                color = new Color("764535"),

                titleColor = Colors.white
            },

            DefaultCardArt = StableSpr.cards_Cannon,
            BorderSprite = RegisterSprite(package, "assets/frame/frame_bruno.png").Sprite,
            Name = AnyLocalizations.Bind(["Bruno", "character", "name"]).Localize
        });

        foreach (KeyValuePair<int, List<string>> anims in PawsaiAnims)
        {
            foreach (string anim in anims.Value)
            {
                RegisterAnimation(BrunoDeck.Deck.Key(), package, anim, $"assets/animation/bruno/bruno_{anim}", anims.Key);
            }
        }

        Bruno = helper.Content.Characters.V2.RegisterPlayableCharacter("bruno", new PlayableCharacterConfigurationV2
        {
            Deck = BrunoDeck.Deck,
            BorderSprite = RegisterSprite(package, "assets/frame/char_frame_bruno.png").Sprite,
            Starters = new StarterDeck
            {
                cards = [
                    new Breacher(),
                    new ToughenUp()
                ],
                artifacts = [
                ]
            },
            Description = AnyLocalizations.Bind(["Bruno", "character", "desc"]).Localize,
            ExeCardType = typeof(BrunoExe)
        });

        MoreDifficultiesApi?.RegisterAltStarters(BrunoDeck.Deck, new StarterDeck
        {
            cards = [
                new HypeUp(),
                new AllIn()
            ],
            artifacts =
            [
            ]
        });

        Status_Recoil = helper.Content.Statuses.RegisterStatus("Recoil", new StatusConfiguration
        {
            Definition = new StatusDef
            {
                isGood = false,
                affectedByTimestop = false,
                color = new Color("00ff00"),
                icon = RegisterSprite(package, "assets/icon/bruno/Status_Recoil.png").Sprite
            },
            Name = AnyLocalizations.Bind(["Bruno", "status", "Recoil", "name"]).Localize,
            Description = AnyLocalizations.Bind(["Bruno", "status", "Recoil", "desc"]).Localize
        });
        Status_Hyperdrive = helper.Content.Statuses.RegisterStatus("Hyperdrive", new StatusConfiguration
        {
            Definition = new StatusDef
            {
                isGood = true,
                affectedByTimestop = false,
                color = new Color("00ff00"),
                icon = RegisterSprite(package, "assets/icon/bruno/Status_Hyperdrive.png").Sprite
            },
            Name = AnyLocalizations.Bind(["Bruno", "status", "Hyperdrive", "name"]).Localize,
            Description = AnyLocalizations.Bind(["Bruno", "status", "Hyperdrive", "desc"]).Localize
        });
        Status_Mitigate = helper.Content.Statuses.RegisterStatus("Mitigate", new StatusConfiguration
        {
            Definition = new StatusDef
            {
                isGood = true,
                affectedByTimestop = true,
                color = new Color("00ff00"),
                icon = RegisterSprite(package, "assets/icon/bruno/Status_Mitigate.png").Sprite
            },
            Name = AnyLocalizations.Bind(["Bruno", "status", "Mitigate", "name"]).Localize,
            Description = AnyLocalizations.Bind(["Bruno", "status", "Mitigate", "desc"]).Localize
        });
        Status_SlowBurn = helper.Content.Statuses.RegisterStatus("SlowBurn", new StatusConfiguration
        {
            Definition = new StatusDef
            {
                isGood = false,
                affectedByTimestop = true,
                color = new Color("00ff00"),
                icon = RegisterSprite(package, "assets/icon/bruno/Status_SlowBurn.png").Sprite
            },
            Name = AnyLocalizations.Bind(["Bruno", "status", "SlowBurn", "name"]).Localize,
            Description = AnyLocalizations.Bind(["Bruno", "status", "SlowBurn", "desc"]).Localize
        });
        Status_Hamper = helper.Content.Statuses.RegisterStatus("Hamper", new StatusConfiguration
        {
            Definition = new StatusDef
            {
                isGood = false,
                affectedByTimestop = false,
                color = new Color("00ff00"),
                icon = RegisterSprite(package, "assets/icon/bruno/Status_Hamper.png").Sprite
            },
            Name = AnyLocalizations.Bind(["Bruno", "status", "Hamper", "name"]).Localize,
            Description = AnyLocalizations.Bind(["Bruno", "status", "Hamper", "desc"]).Localize
        });


        foreach (Type ta in BrunoArtifactTypes)
        {
            Deck deck = BrunoDeck.Deck;
            if (BrunoDuoArtifactTypes.Contains(ta))
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
            helper.Content.Artifacts.RegisterArtifact(ta.Name, UhDuhHundo.ArtifactRegistrationHelper(ta, RegisterSprite(package, "assets/artifact/bruno/" + ta.Name + ".png").Sprite, deck, "Bruno"));
        }

        _ = new Hamper();
        _ = new Hyperdrive();
        _ = new Mitigate();
        _ = new Recoil();
        _ = new SlowBurn();
    }
}