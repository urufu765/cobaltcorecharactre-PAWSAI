using System.Collections.Generic;
using Nanoray.PluginManager;
using Nickel;
using JollyJolly.External;
using static JollyJolly.Conversation.CommonDefinitions;

namespace JollyJolly.Conversation;

internal class EventDialogue : IRegisterable
{
    public static void Register(IPluginPackage<IModManifest> package, IModHelper helper)
    {
        LocalDB.DumpStoryToLocalLocale("en", new Dictionary<string, DialogueMachine>(){
            {"AbandonedShipyard_Repaired", new(){
                edit = [
                    new("7657a54e", AmWeth, "Got some cool samples!")
                ]
            }},
            {$"ChoiceCardRewardOfYourColorChoice_{AmWeth}", new(){
                type = NodeType.@event,
                oncePerRun = true,
                bg = "BGBootSequence",
                dialogue = [
                    new(AmWeth, "surprise", "ACK! Ow?!"),
                    new(AmVoid, "You have been a good girl. Have a crystal."),
                    new(AmWeth, "crystallolipop", "Oh. Yay!"),
                    new(AmCat, "Energy readings are back to normal.")
                ]
            }},
            {"CrystallizedFriendEvent", new(){
                edit = [new("8383e940", AmWeth, "plead", "No...")]
            }},
            {$"CrystallizedFriendEvent_{AmWeth}", new(){
                type = NodeType.@event,
                oncePerRun = true,
                bg = "BGCrystalizedFriend",
                allPresent = [AmWeth],
                dialogue = [
                    new(new Wait{secs = 1.5}),
                    new(AmWeth, "yay", "Hello! I'm ready to adventure!")
                ]
            }},
            {"DraculaTime", new(){
                edit = [new("32916484", AmWeth, "squint", "I think I saw you on television...")]
            }},
            {"EphemeralCardGift", new(){
                edit = [new("db59f595", AmWeth, "pain", "STOP!")]
            }},
            {"ForeignCardOffering_After", new(){
                edit = [new(EMod.countFromStart, 1, AmWeth, "I can make this work. I think.")]
            }},
            {"ForeignCardOffering_Refuse", new(){
                edit = [new(EMod.countFromStart, 1, AmWeth, "tired", "I think I'm dellusional.")]
            }},
            {"GrandmaShop", new(){
                edit = [new(EMod.countFromStart, 1, AmWeth, "plead", "To say sowwy...")]
            }},
            {"Knight_1", new(){
                edit = [new(EMod.countFromEnd, 1, AmWeth, "lockedin", "Cometh at me, I doth not feareth thee!")]
            }},
            {"Knight_Midcombat_Greeting_Weth_Multi_0", new(){
                type = NodeType.@event,
                oncePerCombat = true,
                allPresent = [AmWeth, "knight"],
                lookup = ["knight_duel"],
                requiredScenes = ["Knight_Midcombat_Greeting_1"],
                choiceFunc = "KnightCombatChoices",
                dialogue = [
                    new("knight", "Ah, it is you! Let us partake in anothere <c=keyword>honorable duel</c>!", true),
                    new(AmWeth, "I hesitate to chooseth!")
                ]
            }},
            {"Knight_Midcombat_YouWin", new(){
                dialogue = [
                    new(),
                    new([
                        new(AmWeth, "Thanketh thee f'r this hon'rable duel, farewell!")
                    ])
                ]
            }},
            {"LoseCharacterCard", new(){
                edit = [new("79d6356a", AmWeth, "terror", "Aah! It hurts! What hurt?! Make it stop!!!")]
            }},
            {"LoseCharacterCard_No", new(){
                edit = [new("ea6cfd2f", AmWeth, "traumatised", "...")]
            }},
            {$"LoseCharacterCard_{AmWeth}", new(){
                type = NodeType.@event,
                oncePerRun = true,
                bg = "BGSupernova",
                dialogue = [
                    new(AmWeth, "traumatised", "...")
                ]
            }},
            {"Sasha_2_Multi_2", new(){
                edit = [new("89fa9389", AmWeth, "yay", "Ball!")]
            }},
            {"ShopkeeperInfinite_Weth_Multi_0", new(){
                type = NodeType.@event,
                lookup = ["shopBefore"],
                bg = "BGShop",
                allPresent = [AmWeth],
                dialogue = [
                    new(AmShopkeeper, "Causing troubles again?", true),
                    new(AmWeth, "plead", "No ma'am."),
                    new(new Jump{key = "NewShop"})
                ]
            }},
            {"ShopkeeperInfinite_Weth_Multi_1", new(){
                type = NodeType.@event,
                lookup = ["shopBefore"],
                bg = "BGShop",
                allPresent = [AmWeth],
                dialogue = [
                    new(AmShopkeeper, "Hello there.", true),
                    new(AmWeth, "Hello ma'am"),
                    new(new Jump{key = "NewShop"})
                ]
            }},
            {"ShopkeeperInfinite_Weth_Multi_2", new(){
                type = NodeType.@event,
                lookup = ["shopBefore"],
                bg = "BGShop",
                allPresent = [AmWeth],
                requiredScenes = [ "Weth_Memory_2" ],
                maxHullPercent = 0.25,
                dialogue = [
                    new(AmShopkeeper, "I thought I told you to stop bringing me wreckages.", true),
                    new([
                        new(AmWeth, "facepalm", "It won't happen again..."),
                        new(AmWeth, "sob", "I'm so sorry!!!"),
                        new(AmWeth, "panic", "I was careful this time! I swear!")
                    ]),
                    new(new Jump{key = "NewShop"})
                ]
            }},
            {"SogginsEscape_1", new(){
                edit = [new("af37567d", AmWeth, "Are you wanna die?")]
            }},
            {"Soggins_1", new(){
                edit = [new("28308960", AmWeth, "squint", "I suppose I have the tools necessary to help him.")]
            }},
            {"Soggins_Infinite", new(){
                edit = [new("30e9da01", AmWeth, "We'll see about that.")]
            }},
            {"TheCobalt_Weth_1", new(){
                type = NodeType.@event,
                lookup = ["before_theCobalt"],
                once = true,
                priority = true,
                requiredScenes = ["TheCobalt_2"],
                allPresent = [AmWeth],
                bg = "BGTheCobalt",
                dialogue = [
                    new(AmWeth, "Ah, the Cobalt..."),
                    new(AmWeth, "explain", "It's as beautiful as the first time I laid my eyes on the beaut."),
                    new(AmWeth, "tired", "..."),
                    new(AmCat, "You're shaking."),
                    new(AmWeth, "pain", "I've also developed a bit of a phobia from giant ships transporting giant crystals."),
                    new(AmCat, "squint", "As one does."),
                    new(AmWeth, "squint", "..."),
                    new(AmCat, "..."),
                    new(AmWeth, "hmm", "So... are we just gonna stare at the ship or...?"),
                    new(AmCat, "See ya soon!")
                ]
            }},
            {"Wizard_Weth_1", new(){
                type = NodeType.@event,
                lookup = ["before_wizard"],
                once = true,
                priority = true,
                bg = "BGWizard",
                requiredScenes = ["Wizard_1"],
                allPresent = [AmWeth],
                dialogue = [
                    new(AmWizbo, "Skibidoom! Behold! Wizbo!", true),
                    new(AmWeth, "Wizard stuff?"),
                    new(AmWizbo, "Yes!", true),
                    new(AmWeth, "That's lame."),
                    new(AmWizbo, "Fiddle-dee hee! How dare yee!", true)
                ]
            }},
            {"Wizard_Weth_2", new(){
                type = NodeType.@event,
                lookup = ["before_wizard"],
                once = true,
                priority = true,
                bg = "BGWizard",
                requiredScenes = ["Wizard_Weth_1"],
                allPresent = [AmWeth],
                dialogue = [
                    new(AmWizbo, "Whablamo! Not you again!", true),
                    new(AmWeth, "Wait, before we fight, I'd like to formally apologize."),
                    new(AmWizbo, "Oh? The one-eyed vermin wants to apologize to the great Wizbo?!", true),
                    new(AmWeth, "mad", "..."),
                    new(AmWeth, "mad", "I changed my mind.")
                ]
            }},
            {"Wizard_Weth_3", new(){
                type = NodeType.@event,
                lookup = ["before_wizard"],
                once = true,
                priority = true,
                bg = "BGWizard",
                requiredScenes = ["Wizard_Weth_2"],
                allPresent = [AmWeth],
                dialogue = [
                    new(AmWizbo, "Wizbo here! You here to insult again?", true),
                    new(AmWeth, "Actually, I wanted to say sorry... magic is actually pretty cool..."),
                    new(AmWizbo, "That's it?", true),
                    new(AmWeth, "plead", "No, I would wish to learn more about this magic sometime? Please?"),
                    new(AmWizbo, "Hmm, this great Wizbo accepts your apology AND your wish!", true),
                    new(AmWeth, "sparkle", "Thank you."),
                    new(AmWizbo, "But unfortunately I still have to kill you.", true),
                    new(AmWeth, "sad", "Damn.")
                ]
            }}
        });
    }
}
