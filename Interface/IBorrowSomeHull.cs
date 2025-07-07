namespace Starhunters.API;

/// <summary>
/// Interface for StuffBase stuff that can borrow hull.
/// </summary>
public interface IBorrowSomeHull
{
    public int Health { get; set; }
    public int BaseBorrowAmount();
}