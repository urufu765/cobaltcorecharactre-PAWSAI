namespace Starhunters.Kodijen.Actions;

public class ADestroyOutsideDrones : CardAction
{
    public bool playerDidIt = true;
    public bool checkEnemy;

    public override void Begin(G g, State s, Combat c)
    {
        foreach (int x in c.stuff.Keys)
        {
            if (c.stuff[x].Invincible()) continue;  // Don't touch the invincible ones

            if (checkEnemy)  // enemy
            {
                if (!(c.otherShip.GetPartAtWorldX(x) is Part cp && cp.type != PType.empty))
                {
                    c.DestroyDroneAt(s, x, playerDidIt);
                }
            }
            else  // player
            {
                if (!(s.ship.GetPartAtWorldX(x) is Part sp && sp.type != PType.empty))
                {
                    c.DestroyDroneAt(s, x, playerDidIt);
                }
            }
        }
    }
}