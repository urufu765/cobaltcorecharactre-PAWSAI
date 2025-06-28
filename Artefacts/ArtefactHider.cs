using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Microsoft.Extensions.Logging;

namespace JollyJolly.Artifacts;

public static class Artifacthider
{
    /// <summary>
    /// GetBlockedArtifacts postfix
    /// </summary>
    /// <param name="s"></param>
    /// <param name="__result"></param>
    public static void ArtifactRewardPreventer(State s, ref HashSet<Type> __result)
    {
        try
        {
            // if (!s.EnumerateAllArtifacts().Any(a => a is TreasureHunter) || s.EnumerateAllArtifacts().Any(a => a is TreasureSeeker))
            // {
            //     __result.Add(typeof(TreasureSeeker));
            // }
        }
        catch (Exception err)
        {
            ModEntry.Instance.Logger.LogError(err, "Fuck, fucked up adding stuff to the list of don'ts");
        }
    }
}
