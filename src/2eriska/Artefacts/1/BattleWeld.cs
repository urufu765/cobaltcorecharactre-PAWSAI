using System;

namespace Starhunters.Eriska.Artifacts;

[ArtifactMeta(pools = [ArtifactPool.Common])]
public class BattleWeld : Artifact
{
    public const int TURN_COUNT = 10;


    public override int ModifyHealAmount(int baseAmount, State state, bool targetPlayer)
    {
        if (!targetPlayer) return base.ModifyHealAmount(baseAmount, state, targetPlayer);


        if (state.route is Combat c)
        {
            if (c.turn <= TURN_COUNT)
            {
                return 2;
            }
        }
        else if (baseAmount > 0)
        {
            return -1;
        }
        return base.ModifyHealAmount(baseAmount, state, targetPlayer);
    }

    public override int? GetDisplayNumber(State s)
    {
        if (s.route is Combat c)
        {
            return Math.Max(0, TURN_COUNT - c.turn);
        }
        return base.GetDisplayNumber(s);
    }
}