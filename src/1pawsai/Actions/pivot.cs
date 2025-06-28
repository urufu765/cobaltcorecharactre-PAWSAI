namespace Starhunters.Pawsai.Actions;

public class APivot : AMove
{
    /// <summary>
    /// No.
    /// </summary>
    public new bool fromEvade => false;

    /// <summary>
    /// Doesn't matter whether it targets player or not, it'll do the same thing
    /// </summary>
    public new bool targetPlayer => false;
    
    /// <summary>
    /// To reduce confusion (on my end). Inward movement is positive, outward movement is negative. For readability, amount and outward bool is used instead.
    /// </summary>
    public new int dir => outward ? -amount : amount;
    public int amount;
    public bool outward = false;
}