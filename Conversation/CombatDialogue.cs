using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Nanoray.PluginManager;
using Nickel;
using Starhunters.Artifacts;
using Starhunters.External;
using static Starhunters.Conversation.CommonDefinitions;

namespace Starhunters.Conversation;

internal class CombatDialogue : IRegisterable, IDialogueRegisterable
{
    public static void Register(IPluginPackage<IModManifest> package, IModHelper helper)
    {
        LocalDB.DumpStoryToLocalLocale("en", new Dictionary<string, DialogueMachine>(){
        });
    }

    public static void LateRegister()
    {
    }
}
