using Starhunters.API;

namespace Starhunters.Kodijen.Objects;

// TODO: Figure out a solution for Catch
public class MiniatureArtemis : StuffBase, IBorrowSomeHull
{
    public int Health { get; set; }
    public int BaseBorrowAmount()
    {
        return 5;
    }
}