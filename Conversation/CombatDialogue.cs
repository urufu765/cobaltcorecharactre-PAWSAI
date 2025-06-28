using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Nanoray.PluginManager;
using Nickel;
using JollyJolly.Artifacts;
using JollyJolly.External;
using static JollyJolly.Conversation.CommonDefinitions;

namespace JollyJolly.Conversation;

internal class CombatDialogue : IRegisterable, IDialogueRegisterable
{
    public static void Register(IPluginPackage<IModManifest> package, IModHelper helper)
    {
        LocalDB.DumpStoryToLocalLocale("en", new Dictionary<string, DialogueMachine>(){
            {"Example_JollyJolly_0", new(){
                type = NodeType.combat,
                enemyShotJustHit = true,
                minDamageBlockedByPlayerArmorThisTurn = 3,
                oncePerCombatTags = ["YowzaThatWasALOTofArmorBlock"],
                oncePerRun = true,
                allPresent = [AmWeth],
                dialogue = [
                    new(AmWeth, "think", "Okay, reminder to self, don't do what the enemy is doing...")
                ]
            }},
        });
    }

    public static void LateRegister()
    {
    }
}
