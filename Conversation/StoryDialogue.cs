using System.Collections.Generic;
using Nanoray.PluginManager;
using Nickel;
using JollyJolly.Artifacts;
using JollyJolly.External;
using static JollyJolly.Conversation.CommonDefinitions;

namespace JollyJolly.Conversation;

internal class StoryDialogue : IRegisterable
{
    public static void Register(IPluginPackage<IModManifest> package, IModHelper helper)
    {
        LocalDB.DumpStoryToLocalLocale("en", new Dictionary<string, DialogueMachine>()
        {
            {"Weth_1", new(){
                type = NodeType.@event,
                lookup = ["zone_first"],
                bg = "BGRunStart",
                allPresent = [AmWeth],
                priority = true,
                once = true,
                dialogue = [
                    new(AmWeth, "tired", "..."),
                    new(AmCat, "Hey, who let the dogs in?"),
                    new(AmWeth, "squint", "Where am I?"),
                    new(AmCat, "A ship!"),
                    new(AmWeth, "Fair nuff... where we going?"),
                    new(AmCat, "To the Cobalt!"),
                    new(AmWeth, "The Cobalt?!"),
                    new(AmWeth, "sparkle", "Sign me right up!")
                ]
            }},
            {"Weth_Peri_1", new(){
                type = NodeType.@event,
                lookup = ["zone_first"],
                bg = "BGRunStart",
                allPresent = [AmWeth, AmPeri],
                priority = true,
                once = true,
                dialogue = [
                    new(AmWeth, "tired", "..."),
                    new(AmPeri, "squint", "...", true),
                    new(AmWeth, "squint", "..."),
                    new(AmPeri, "squint", "You're not on the ship manifest.", true),
                    new(AmWeth, "squint", "I have no idea what that is."),
                    new(AmPeri, "squint", "...", true),
                    new(AmWeth, "squint", "..."),
                    new(AmPeri, "squint", "...", true),
                    new(AmWeth, "tired", "Okay my eyes are starting to hurt from all that squinting."),
                    new(AmWeth, "squintoffscreen", "Move over."),
                    new(AmPeri, "mad", "What? Hey! What are you doing?!", true),
                    new(AmWeth, "Manning the guns? What are YOU doing?"),
                    new(AmPeri, "mad", "You're in my seat.", true),
                    new(AmWeth, "explain", "How about we discuss the seating problems... after we blow up that ship that's in front of us?"),
                    new(AmPeri, "squint", "...", true),
                    new(AmPeri, "squint", "Fine.", true)
                ]
            }},
            {"Weth_Peri_2", new(){
                type = NodeType.@event,
                lookup = ["after_crystal"],
                bg = "BGCrystalNebula",
                allPresent = [AmWeth, AmPeri],
                once = true,
                priority = true,
                requiredScenes = ["Weth_Peri_1"],
                dialogue = [
                    new(AmPeri, "Time's up.", true),
                    new(AmWeth, "What's up?"),
                    new(AmPeri, "Get out of my seat.", true),
                    new(AmWeth, "But I like this spot..."),
                    new(AmPeri, "mad", "Off.", true),
                    new(AmWeth, "sad", "..."),
                    new(AmPeri, "mad", "...", true),
                    new(AmWeth, "sad", "..."),
                    new(AmPeri, "squint", "Fine, it's yours.", true),
                    new(AmWeth, "yay", "Yay!")
                ]
            }},
            {"Weth_Dizzy_1", new(){
                type = NodeType.@event,
                lookup = ["zone_first"],
                bg = "BGRunStart",
                allPresent = [AmWeth, AmDizzy],
                priority = true,
                once = true,
                dialogue = [
                    new(AmWeth, "tired", "..."),
                    new(AmWeth, "..."),
                    new(AmWeth, "squint", "What are you doing?"),
                    new(AmDizzy, "You have a crystal growing out of your eye.", true),
                    new(AmWeth, "touch", "..."),
                    new(AmWeth, "Wow, you're right."),
                    new(AmDizzy, "Can I break a chunk off?", true),
                    new(AmWeth, "What?"),
                    new(AmDizzy, "Can I break a chunk off your eye crystal?", true),
                    new(AmWeth, "That sounds incredibly painful..."),
                    new(AmDizzy, "So that's a no?", true),
                    new(AmWeth, "squint", "It's a maybe.")
                ]
            }},
            {"Weth_Dizzy_2", new(){
                type = NodeType.@event,
                lookup = ["after_crystal"],
                bg = "BGCrystalNebula",
                allPresent = [AmWeth, AmDizzy],
                once = true,
                requiredScenes = ["Weth_Dizzy_1", "PostCrystal_1"],
                dialogue = [
                    new(AmWeth, "What do you need my eye crystal for anyways?"),
                    new(AmDizzy, "Science!", true),
                    new(AmWeth, "What kind of science?"),
                    new(AmDizzy, "I don't know!", true),
                    new(AmWeth, "squint", "..."),
                    new(AmWeth, "squint", "I change my mind, you can't have it."),
                    new(AmDizzy, "squint", "Drat.", true)
                ]
            }},
            {"Weth_Dizzy_3", new(){
                type = NodeType.@event,
                lookup = ["after_crystal"],
                bg = "BGCrystalNebula",
                allPresent = [AmWeth, AmDizzy],
                once = true,
                requiredScenes = ["Weth_Dizzy_2", "PostCrystal_1"],
                dialogue = [
                    new(AmDizzy, "crystal", "...", true),
                    new(AmWeth, "squint", "..."),
                    new(AmWeth, "squint", "Why are you holding that crystal chunk next to my head?"),
                    new(AmDizzy, "Science!", true),
                    new(AmWeth, "I'd like to inform you it's also hurting me."),
                    new(AmDizzy, "crystal", "How so?", true),
                    new(AmWeth, "tired", "I have a massive headache, my ears are ringing, and the area around my left eye is aching like crazy."),
                    new(AmDizzy, "crystal", "Interesting...", true),
                    new(AmWeth, "pain", "Please take that away from me, before I violently throw you across the room."),
                    new(AmDizzy, "intense", "Okay.", true)
                ]
            }},
            {"Weth_Max_1", new(){
                type = NodeType.@event,
                lookup = ["after_crystal"],
                bg = "BGCrystalNebula",
                allPresent = [AmWeth, AmMax],
                once = true,
                priority = true,
                hasArtifactTypes = [typeof(SpaceRelics)],
                dialogue = [
                    new(AmMax, "Hey Weth?", true),
                    new(AmWeth, "Hmm?"),
                    new(AmMax, "Can I see that crystal?", true),
                    new(AmWeth, "crystallolipop", "Sure!"),
                    new(AmWeth, "May I ask what for?"),
                    new(AmMax, "To take a picture of it and record it.", true),
                    new(AmWeth, "crystal", "..."),
                    new(AmMax, "intense", "...", true),
                    new(AmMax, "intense", "Hey umm if you just hold it like that I can...", true),
                    new(AmWeth, "..."),
                    new(AmMax, "intense", "W-where did it go?", true),
                    new(AmWeth, "explain", "Sometimes, some things are just not meant to be recorded electronically.")
                ]
            }},
            {"Weth_Books_1", new(){
                type = NodeType.@event,
                lookup = ["zone_first"],
                bg = "BGRunStart",
                allPresent = [AmWeth, AmBooks],
                priority = true,
                once = true,
                requiredScenes = ["Books_1", "RunWinWho_Weth_2"],
                dialogue = [
                    new(AmWeth, "touch", "..."),
                    new(AmBooks, "Umm, hello Miss Weth!", true),
                    new(AmWeth, "Hmm? Hey. What's up?"),
                    new(AmBooks, "You have a lot of cool stuff!", true),
                    new(AmWeth, "explain", "Ah that's just the things I managed to snatch during my ventures."),
                    new(AmWeth, "If you have time, I could tell you all about the fascinating discoveries I've encountered."),
                    new(AmBooks, "Okay!", true),
                    new(AmWeth, "..."),
                    new(AmWeth, "Before that, can I ask a favor?"),
                    new(AmBooks, "Sure!", true),
                    new(AmWeth, "..."),
                    new(AmWeth, "pain", "..."),
                    new(AmWeth, "tired", "..."),
                    new(AmWeth, "Nevermind. Forget about it. Wanna hear about the time I encountered a giant space octopus?"),
                    new(AmBooks, "Yes ma'am!", true)
                ]
            }},

        });
    }
}