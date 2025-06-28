namespace Starhunters.Pawsai.Actions;

public class AMirrorMove : AMove
{
    /// <summary>
    /// Doesn't matter if it targets player or not, both are moving at the same time lol
    /// </summary>
    public new bool targetPlayer => false;
}