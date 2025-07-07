using Starhunters.API;

namespace Starhunters.Kodijen.Objects;

public class SmartDrone : StuffBase, IBorrowSomeHull
{
    public bool upgraded;

    public int Health { get; set; }

    public int BaseBorrowAmount()
    {
        return upgraded ? 3 : 2;
    }
}