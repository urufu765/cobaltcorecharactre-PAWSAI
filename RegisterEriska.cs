using HarmonyLib;
using Nickel;
using Nanoray.PluginManager;
using Starhunters.Eriska.Statuses;
using System;
using Starhunters.Eriska.Cards;
using System.Collections.Generic;

namespace Starhunters;

internal partial class ModEntry : SimpleMod
{
    public void Register_Eriska(IPluginPackage<IModManifest> package, IModHelper helper)
    {
        EriskaDeck = helper.Content.Decks.RegisterDeck("eriska", new DeckConfiguration
        {
            Definition = new DeckDef
            {
                color = new Color(""),

                titleColor = Colors.white
            },

            DefaultCardArt = StableSpr.cards_Cannon,
            BorderSprite = RegisterSprite(package, "assets/frame/frame_eriska.png").Sprite,
            Name = AnyLocalizations.Bind(["Eriska", "character", "name"]).Localize
        });

        foreach (KeyValuePair<int, List<string>> anims in EriskaAnims)
        {
            foreach (string anim in anims.Value)
            {
                RegisterAnimation(EriskaDeck.Deck.Key(), package, anim, $"assets/animation/eriska/eriska_{anim}", anims.Key);
            }
        }

        Eriska = helper.Content.Characters.V2.RegisterPlayableCharacter("eriska", new PlayableCharacterConfigurationV2
        {
            Deck = EriskaDeck.Deck,
            BorderSprite = RegisterSprite(package, "assets/frame/char_frame_eriska.png").Sprite,
            Starters = new StarterDeck
            {
                cards = [
                ],
                artifacts = [
                ]
            },
            Description = AnyLocalizations.Bind(["Eriska", "character", "desc"]).Localize,
            ExeCardType = typeof(EriskaExe)
        });

        MoreDifficultiesApi?.RegisterAltStarters(EriskaDeck.Deck, new StarterDeck
        {
            cards = [
            ],
            artifacts =
            [
            ]
        });

        Status_Burn = helper.Content.Statuses.RegisterStatus("Burn", new StatusConfiguration
        {
            Definition = new StatusDef
            {
                isGood = false,
                affectedByTimestop = true,
                color = new Color("00ff00"),
                icon = RegisterSprite(package, "assets/icon/eriska/Status_Burn.png").Sprite
            },
            Name = AnyLocalizations.Bind(["Eriska", "status", "Burn", "name"]).Localize,
            Description = AnyLocalizations.Bind(["Eriska", "status", "Burn", "desc"]).Localize
        });
        Status_Blister = helper.Content.Statuses.RegisterStatus("Blister", new StatusConfiguration
        {
            Definition = new StatusDef
            {
                isGood = false,
                affectedByTimestop = true,
                color = new Color("00ff00"),
                icon = RegisterSprite(package, "assets/icon/eriska/Status_Blister.png").Sprite
            },
            Name = AnyLocalizations.Bind(["Eriska", "status", "Blister", "name"]).Localize,
            Description = AnyLocalizations.Bind(["Eriska", "status", "Blister", "desc"]).Localize
        });


        foreach (Type ta in EriskaArtifactTypes)
        {
            Deck deck = EriskaDeck.Deck;
            if (EriskaDuoArtifactTypes.Contains(ta))
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
            helper.Content.Artifacts.RegisterArtifact(ta.Name, UhDuhHundo.ArtifactRegistrationHelper(ta, RegisterSprite(package, "assets/artifact/eriska/" + ta.Name + ".png").Sprite, deck, "Eriska"));
        }

    }
}