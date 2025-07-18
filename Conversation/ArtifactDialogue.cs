using System.Collections.Generic;
using Nanoray.PluginManager;
using Nickel;
using Starhunters.Artifacts;
using Starhunters.External;
using static Starhunters.Conversation.CommonDefinitions;

namespace Starhunters.Conversation;

internal class ArtifactDialogue : IRegisterable
{
    public static void Register(IPluginPackage<IModManifest> package, IModHelper helper)
    {
        LocalDB.DumpStoryToLocalLocale("en", new Dictionary<string, DialogueMachine>(){
        });

        LocalDB.DumpStoryToLocalLocale("en", "Shockah.DuoArtifacts", new Dictionary<string, DialogueMachine>(){
        });

    }
}
