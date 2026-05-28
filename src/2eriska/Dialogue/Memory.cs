using System.Collections.Generic;
using Nanoray.PluginManager;
using Nickel;
using Starhunters.Artifacts;
using Starhunters.External;
using Starhunters.Kodijen.Cards;
using Weth.Cards;
using static Starhunters.Conversation.CommonDefinitions;

namespace Starhunters.Eriska.Conversation;

internal class EriskaMemoryDialogue : IRegisterable
{
    public static void Register(IPluginPackage<IModManifest> package, IModHelper helper)
    {
        LocalDB.DumpStoryToLocalLocale("en", new Dictionary<string, DialogueMachine>()
        {
            {"RunWinWho_Eriska_1", new(){
                type = NodeType.@event,
                introDelay = false,
                allPresent = [AmEriska],
                bg = "BGRunWin",
                lookup = [
                    $"runWin_{AmEriska}"
                ],
                dialogue = [
                    new(new Wait{secs = 3}),
                    new(AmEriska, "think", "..."),
                    new(AmVoid, "...", true),
                    new(AmEriska, "squint", "..."),
                    new(AmVoid, "You seem preoccupied.", true),
                    new(AmEriska, "write", "..."),
                    new(AmVoid, "...", true),
                    new(AmEriska, "pointout", "Are you flammable?"),
                    new(AmVoid, "...", true),
                    new(AmEriska, "write", "I'll take that as a no."),
                    new(AmVoid, "Perhaps next time.", true)
                ]
            }},
            {"Eriska_Memory_1", new(){
                type = NodeType.@event,
                introDelay = false,
                bg = "BGHomeSweetHome",
                lookup = [
                    "vault",
                    $"vault_{AmEriska}"
                ],
                dialogue = [
                    new("T-8035 days"),
                    new(new Wait{secs = 2}),
                    new(title: null),
                    new(new Wait{secs = 1 }),
                    new(AmEriska, "teenangst", "You can't do this to me!"),
                    new(AmEriskaMom, "Watch your tone!", true),
                    new(AmEriskaMum, "That is no way to talk to your mother.", true),
                    new(AmEriska, "teenwhy", "Why do you even care?"),
                    new(AmEriska, "teenshout", "I'm doing this with MY OWN MONEY!"),
                    new(AmEriskaMum, "It's for your own good.", true),
                    new(AmEriskaMom, "Why can't you just study like a normal kid?", true),
                    new(AmEriska, "teenteary", "Why can't you ever support anything I do?"),
                    new(AmEriskaMom, "Well if you got good grades...", true),
                    new(AmEriska, "teentearyshout", "NO! I'm DONE! I'm hunting the star whether you like it or not!"),
                    new(AmEriskaMum, "Hey! Not the window!", true),
                    new(AmEriskaMum, "..."),
                    new(AmEriskaMom, "...", true),
                    new(AmEriskaMum, "Do you think we were too harsh on her?"),
                    new(AmEriskaMom, "It's probably her hormones doing the talking. She'll come to her senses soon.", true),
                    new(AmEriskaMum, "... I hope so.")
                ]
            }},
            {"Eriska_Memory_2", new(){
                type = NodeType.@event,
                introDelay = false,
                bg = "BGDesolateWorld",
                lookup = [
                    "vault",
                    $"vault_{AmEriska}"
                ],
                dialogue = [
                    new("T-365 days"),
                    new(new Wait{secs = 2}),
                    new(title: null),
                    new(new Wait{secs = 5}),
                    new(AmEriska, "Hey mums. Guess what?"),
                    new(AmEriska, "It's that day of the year again..."),
                    new(new Wait{secs = 3}),
                    new(AmEriska, "awkward", "Sorry I couldn't visit last year... or the year before that... for a few years..."),
                    new(AmEriska, "You know how it is, being a star hunter and all... another faraway expedition..."),
                    new(new Wait{secs = 3}),
                    new(new BGAction{action = "sit down"}),
                    new(new Wait{secs = 4}),
                    new(AmEriska, "explain", "I know you still probably disapprove of me going this route and..."),
                    new(AmEriska, "down", "... I probably should've wrote home at least once..."),
                    new(new Wait{secs = 1}),
                    new(new BGAction{action = "lean back"}),
                    new(new Wait{secs = 2}),
                    new(AmEriska, "up", "I'm such an awful daughter, aren't I?"),
                    new(AmEriska, "upwince", "I mean what kind of daughter runs away from home and hitchhikes her way across the galaxy?"),
                    new(AmEriska, "explain", "To chase after the wishing star, a storybook legend, that grants any wish."),
                    new(new Wait{secs = 4}),
                    new(AmEriska, "chuckle", "Psh. All so I could wish for that approval you would never give."),
                    new(AmEriska, "up", "What was I thinking?"),
                    new(new Wait{secs = 2}),
                    new(new BGAction{action = "wrap arms"}),
                    new(new Wait{secs = 3}),
                    new(AmEriska, "down", "It's time I admit that... I'll never find the star."),
                    new(AmEriska, "theatre", "Even if my new wish is to reverse the... event that happened on this day..."),
                    new(new Wait{secs = 2}),
                    new(AmEriska, "But! I made new friends, found a new purpose..."),
                    new(AmEriska, "awkwardsmile", "Aren't you satisfied that at least your daughter is happy now?"),
                    new(new Wait{secs = 2}),
                    new(new BGAction{action = "nuh uh"}),
                    new(new Wait{secs = 8}),
                    new(AmEriska, "downunamused", "..."),
                    new(AmEriska, "downdisappointed", "Of course not.")
                ]
            }},
            {"Eriska_Memory_3", new(){
                type = NodeType.@event,
                introDelay = false,
                bg = "BGRocketGrills",
                lookup = [
                    "vault",
                    $"vault_{AmEriska}"
                ],
                dialogue = [
                    new("T-1208 days"),
                    new(new Wait{secs = 2}),
                    new(title: null),
                    new(new Wait{secs = 1 }),
                    new(AmBruno, "eyebrow", "So, what makes you qualified for this position?", true),
                    new(AmEriska, "smug", "Read my file. Let me know if you have any questions."),
                    new(AmBruno, "glare", "...", true),
                    new(AmBruno, "reading", "Actually, I did have some... questions.", true),
                    new(AmBruno, "frown", "Criminal history?", true),
                    new(AmEriska, "laidback", "Appealed and retracted."),
                    new(AmBruno, "writing", "Bold of you to self-report.", true),
                    new(AmEriska, "shrug", "On the other hand, makes me that much more trustworthy aye?"),
                    new(AmBruno, "eyebrow", "...", true),
                    new(AmBruno, "eyebrow", "43 cases of arson.", true),
                    new(AmEriska, "challenge", "All thrown out due to lack of evidence."),
                    new(AmBruno, "reading", "Slaughter suspect? Involved in the destruction of an entire organization?", true),
                    new(AmEriska, "lookoff", "Those good for nothing scum had it coming."),
                    new(AmBruno, "eyebrow", "...", true),
                    new(AmEriska, "chintap", "I won't lie, things happened to lined up perfectly in such a way to make me look bad."),
                    new(AmEriska, "explain", "I just happened to be there with a suspiciously damning motive, and a rock solid alibi."),
                    new(AmEriska, "pointout", "I'm actually listed as a witness, if you read a bit further."),
                    new(AmBruno, "readinghard", "...", true),
                    new(AmBruno, "reading", "Ah, yes...", true),
                    new(AmBruno, "squint", "...", true),
                    new(AmBruno, "squint", "You do realize you're applying for the janitor position, right?", true),
                    new(AmEriska, "laidback", "I know. I'm saying I'm very qualified for this position."),
                    new(AmBruno, "squint", "You have zero janitorial experience.", true),
                    new(AmEriska, "smug", "Aha! But! Consider! I'm very good at making things disappear real quick, without a trace."),
                    new(AmEriska, "explain", "If that doesn't scream qualification, I don't know what will."),
                    new(AmBruno, "eyerub", "...", true),
                    new(AmBruno, "donewithit", "I'll be in contact with you within a week, goodbye.", true)
                ]
            }},
        });
    }
}