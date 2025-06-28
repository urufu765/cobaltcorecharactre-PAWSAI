using System.Collections.Generic;
using Nanoray.PluginManager;
using Nickel;
using JollyJolly.Artifacts;
using JollyJolly.External;
using static JollyJolly.Conversation.CommonDefinitions;

namespace JollyJolly.Conversation;

internal class ArtifactDialogue : IRegisterable
{
    public static void Register(IPluginPackage<IModManifest> package, IModHelper helper)
    {
        LocalDB.DumpStoryToLocalLocale("en", new Dictionary<string, DialogueMachine>(){
            {"AritfactExample_JollyJolly_0", new(){
                type = NodeType.combat,
                turnStart = true,
                maxTurnsThisCombat = 1,
                hasArtifacts = ["AresCannonV2"],
                oncePerRunTags = ["AresCannonV2"],
                allPresent = [AmWeth],
                dialogue = [
                    new(AmWeth, "sly", "Now that's what I'm talking about!")
                ]
            }},
        });

        LocalDB.DumpStoryToLocalLocale("en", "Shockah.DuoArtifacts", new Dictionary<string, DialogueMachine>(){
            {"AritfactExample_JollyJolly_1", new(){
                type = NodeType.combat,
                turnStart = true,
                maxTurnsThisCombat = 1,
                hasArtifacts = ["AresCannonV2"],
                oncePerRunTags = ["AresCannonV2"],
                allPresent = [AmWeth],
                dialogue = [
                    new(AmWeth, "sly", "Now that's what I'm talking about!")
                ]
            }},
        });

    }
}
