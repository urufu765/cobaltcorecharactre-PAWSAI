using HarmonyLib;
using Microsoft.Extensions.Logging;
using Nanoray.PluginManager;
using Nickel;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JollyJolly.Artifacts;
// using JollyJolly.Cards;
using JollyJolly.External;
// using JollyJolly.Features;
using System.Reflection;
// using JollyJolly.Actions;
using JollyJolly.Conversation;
//using System.Reflection;

namespace JollyJolly;

internal partial class ModEntry : SimpleMod
{
    private static void Apply(Harmony harmony)
    {
        // Artifacthider
        harmony.Patch(
            original: typeof(ArtifactReward).GetMethod("GetBlockedArtifacts", AccessTools.all),
            postfix: new HarmonyMethod(typeof(Artifacthider), nameof(Artifacthider.ArtifactRewardPreventer))
        );
    }
}