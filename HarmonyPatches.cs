using HarmonyLib;
using Microsoft.Extensions.Logging;
using Nanoray.PluginManager;
using Nickel;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Starhunters.Artifacts;
// using JollyJolly.Cards;
using Starhunters.External;
// using JollyJolly.Features;
using System.Reflection;
// using JollyJolly.Actions;
using Starhunters.Conversation;
using Starhunters.Pawsai.Actions;
using Starhunters.Pawsai.Artifacts;
using Starhunters.Bruno.Actions;
//using System.Reflection;

namespace Starhunters;

internal partial class ModEntry : SimpleMod
{
    private static void Apply(Harmony harmony)
    {
        // Artifacthider
        // harmony.Patch(
        //     original: typeof(ArtifactReward).GetMethod("GetBlockedArtifacts", AccessTools.all),
        //     postfix: new HarmonyMethod(typeof(Artifacthider), nameof(Artifacthider.ArtifactRewardPreventer))
        // );

        // Movement flipper
        harmony.Patch(
            original: typeof(Card).GetMethod(nameof(Card.GetActionsOverridden), AccessTools.all),
            postfix: new HarmonyMethod(typeof(PawsaiMovingHelper), nameof(PawsaiMovingHelper.FlipThemActionsOverriden))
        );

        harmony.Patch(
            original: typeof(Ship).GetMethod(nameof(Ship.DirectHullDamage), AccessTools.all),
            prefix: new HarmonyMethod(typeof(PainCruiserHelper), nameof(PainCruiserHelper.HopeYouLikePain))
        );

        harmony.Patch(
            original: typeof(AAttack).GetMethod(nameof(AAttack.Begin), AccessTools.all),
            prefix: new HarmonyMethod(typeof(BrunoAttackHelper), nameof(BrunoAttackHelper.DoExtraAttackStuff)),
            postfix: new HarmonyMethod(typeof(BrunoAttackHelper), nameof(BrunoAttackHelper.SlapSomeRecoil))
        );
    }
}