using System.Collections.Generic;
using Nanoray.PluginManager;
using Nickel;
using JollyJolly.Artifacts;
using JollyJolly.External;
using static JollyJolly.Conversation.CommonDefinitions;

namespace JollyJolly.Conversation;

internal class CardDialogue : IRegisterable
{
    public static void Register(IPluginPackage<IModManifest> package, IModHelper helper)
    {
        LocalDB.DumpStoryToLocalLocale("en", new Dictionary<string, DialogueMachine>()
        {
        });
    }
}