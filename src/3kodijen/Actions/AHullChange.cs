using System;

namespace Starhunters.Kodijen.Actions;

public class AHullChange : AHullMax
{
    public override void Begin(G g, State s, Combat c)
    {
        Ship? ship = targetPlayer ? s.ship : c.otherShip;

        if (ship == null)
        {
            return;
        }

        ship.hull = Math.Max(1, ship.hull + amount);
        base.Begin(g, s, c);
    }
}