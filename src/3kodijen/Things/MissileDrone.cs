using System.Collections.Generic;

namespace Starhunters.Kodijen.Objects;

// May require separate support for modded missiles
public class MissileDrone : StuffBase
{
    public List<Missile> missiles;
    public bool upgraded;

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
}