using System.Collections.Generic;

namespace Starhunters.Bruno.Artifacts;

[ArtifactMeta(pools = new[] { ArtifactPool.Common })]
public class UnfairAdvantage : Artifact
{
    public bool AdvantageActive { get; set; }
    public bool OutOfCombatMode { get; set; }


    public override Spr GetSprite()
    {
        if (!OutOfCombatMode && !AdvantageActive)
        {
            return ModEntry.Instance.UnfairAdvantage_Depleted;
        }
        return base.GetSprite();
    }


    public override void OnCombatStart(State state, Combat combat)
    {
        OutOfCombatMode = false;
        if (combat.otherShip.GetMaxShield() > 0)
        {
            AdvantageActive = true;
        }
    }

    public override void OnCombatEnd(State state)
    {
        OutOfCombatMode = true;
    }

    public override List<Tooltip>? GetExtraTooltips()
    {
        return [new TTGlossary("status.shieldAlt")];
    }
}