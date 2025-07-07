using System.Collections.Generic;
using Starhunters.API;

namespace Starhunters.Kodijen.Objects;

// May require separate support for modded missiles
public class MissileDrone : StuffBase, IBorrowSomeHull
{
    public List<Missile> missiles;
    public bool upgraded;
    public int Health { get; set; }

    public MissileDrone()
    {
        missiles = upgraded ?
        [
            new Missile{missileType = MissileType.normal},
            new Missile{missileType = MissileType.normal},
            new Missile{missileType = MissileType.normal}
        ] : [
            new Missile{missileType = MissileType.seeker},
            new Missile{missileType = MissileType.seeker},
            new Missile{missileType = MissileType.seeker}
        ];
    }

    public int BaseBorrowAmount()
    {
        return 1;
    }
}