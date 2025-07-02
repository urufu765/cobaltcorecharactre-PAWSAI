namespace Starhunters.Bruno.Actions;

public class ADestroyEntireHand : CardAction
{
    public override void Begin(G g, State s, Combat c)
    {
        timer = 0;
        foreach (Card card in c.hand)
        {
            c.Queue(new AFancyDestroyCard
            {
                uuid = card.uuid
            });
        }
    }
}