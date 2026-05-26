namespace Starhunters.API;

public interface IArtifactModifyBurnBlisterBaseDamage
{
    /// <summary>
    /// Modifies the base damage for burn by adding/subbing from the base value of 1.
    /// </summary>
    /// <param name="state">State</param>
    /// <param name="combat">Combat</param>
    /// <param name="targetPlayer">Whether it does more damage on player ship (true), or on enemy ship (false)</param>
    /// <returns>number to increase/decrease base damage by</returns>
    public int ModifyBurnBaseDamage(State state, Combat combat, bool targetPlayer);

    /// <summary>
    /// Modifies the base damage for blister by adding/subbing from the base value of 1.
    /// </summary>
    /// <param name="state">State</param>
    /// <param name="combat">Combat</param>
    /// <param name="targetPlayer">Whether it does more damage on player ship (true), or on enemy ship (false)</param>
    /// <returns>number to increase/decrease base damage by</returns>
    public int ModifyBlisterBaseDamage(State state, Combat combat, bool targetPlayer);
}