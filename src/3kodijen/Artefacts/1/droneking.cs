using System.Collections.Generic;
using Nickel;

namespace Starhunters.Kodijen.Artifacts;

public enum DroneKingDifficulty
{
    Easy_MD,
    Normal,
    Hard,
    Harder,
    Hardest,
    Brutal_MD,
    Cosmic_MD
}

[ArtifactMeta(pools = new[] { ArtifactPool.Common })]
public class BattleDroneBoast : Artifact
{
    public bool GitGud { get; set; }
    public DroneKingDifficulty difficulty = DroneKingDifficulty.Normal;


    public static double GetTolerance(DroneKingDifficulty difficulty)
    {
        return difficulty switch
        {
            DroneKingDifficulty.Easy_MD => 0.5,
            DroneKingDifficulty.Normal => 0.75,
            DroneKingDifficulty.Hard => 0.9,
            DroneKingDifficulty.Harder => 0.95,
            _ => 1
        };
    }

    public override void OnCombatStart(State state, Combat combat)
    {
        double startPercent = (double)state.ship.hull / state.ship.hullMax;

        if (state.EnumerateAllArtifacts().Find(a => a is HARDMODE) is HARDMODE hm)
        {
            if (ModEntry.Instance.MoreDifficultiesApi is not null)
            {
                difficulty = hm.difficulty switch
                {
                    -1 => DroneKingDifficulty.Easy_MD,
                    1 => DroneKingDifficulty.Hard,
                    2 => DroneKingDifficulty.Harder,
                    3 => DroneKingDifficulty.Hardest,
                    4 => DroneKingDifficulty.Brutal_MD,
                    5 => DroneKingDifficulty.Cosmic_MD,
                    _ => DroneKingDifficulty.Normal
                };
            }
            else
            {
                difficulty = hm.difficulty switch
                {
                    1 => DroneKingDifficulty.Hard,
                    2 => DroneKingDifficulty.Harder,
                    3 => DroneKingDifficulty.Hardest,
                    _ => DroneKingDifficulty.Normal
                };
            }
        }
        else
        {
            difficulty = DroneKingDifficulty.Normal;
        }

        GitGud = startPercent < GetTolerance(difficulty);
    }

    public override void OnPlayerLoseHull(State state, Combat combat, int amount)
    {
        if (!GitGud && (double)(state.ship.hull - amount) / state.ship.hullMax < GetTolerance(difficulty))
        {
            GitGud = true;
            Pulse();
        }
    }

    public override void OnTurnStart(State state, Combat combat)
    {
        if (GitGud)
        {
            combat.QueueImmediate(new AEnergy
            {
                changeAmount = -1,
                artifactPulse = Key()
            });
        }
        else
        {
            combat.QueueImmediate(new AEnergy
            {
                changeAmount = 1,
                artifactPulse = Key()
            });
        }
    }

    public override Spr GetSprite()
    {
        return GitGud ? ModEntry.Instance.BattleDroneBoast_Fail : base.GetSprite();
    }
}

public static class BoastAllTheWay
{
    public static void TokenTheTooltips(ref List<Tooltip> __result, Artifact __instance)
    {
        if (__instance is BattleDroneBoast bd)
        {
            __result[0] = new GlossaryTooltip("battleDroneBoast")
            {
                Title = ModEntry.Instance.Localizations.Localize(["Kodijen", "artifact", "Common", "BattleDroneBoast", "name"]),
                TitleColor = Colors.artifact,
                Description = ModEntry.Instance.Localizations.Localize(["Kodijen", "artifact", "Common", "BattleDroneBoast", "desc"], new
                {
                    difficulty = (int)(BattleDroneBoast.GetTolerance(bd.difficulty) * 100)
                })
            };
        }
    }
}