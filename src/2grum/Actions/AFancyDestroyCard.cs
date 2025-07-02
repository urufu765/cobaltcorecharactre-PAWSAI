using FSPRO;

namespace Starhunters.Bruno.Actions;

public class AFancyDestroyCard : ADestroyCard
{
    public override void Begin(G g, State s, Combat c)
    {
        timer = 0.0;
        Card? card = s.FindCard(uuid);
        if (card is not null && c.hand.Contains(card))
        {
            card.ExhaustFX();
            Audio.Play(Event.CardHandling);
            base.Begin(g, s, c);
            timer = 0.3;
        }
    }
}