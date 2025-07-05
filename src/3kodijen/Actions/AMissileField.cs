using System.Collections.Generic;
using FSPRO;
using Nickel;

namespace Starhunters.Kodijen.Actions;

public class AMissileField : CardAction
{
    public required Missile missile;

    public override void Begin(G g, State s, Combat c)
    {
        foreach (StuffBase stuffBase in c.stuff.Values)
        {
            c.stuff.Remove(stuffBase.x);
            Missile replacement = Mutil.DeepCopy(missile);
            replacement.x = stuffBase.x;
            replacement.xLerped = stuffBase.xLerped;
            replacement.bubbleShield = stuffBase.bubbleShield;
            replacement.targetPlayer = stuffBase.targetPlayer;
            replacement.age = stuffBase.age;
            c.stuff[stuffBase.x] = replacement;
        }
        Audio.Play(Event.Status_PowerDown, true);
    }

    public override List<Tooltip> GetTooltips(State s)
    {
        if (s.route is Combat c)
        {
            foreach (StuffBase stuffBase in c.stuff.Values)
            {
                stuffBase.hilight = 2;
            }
        }

        return
        [
            new GlossaryTooltip("breachAttackPierce")
            {
                Title = ModEntry.Instance.Localizations.Localize(["Kodijen", "action", "MissileField", "title"]),
                Icon = ModEntry.Instance.Action_MissileField,
                Description = ModEntry.Instance.Localizations.Localize(["Kodijen", "action", "MissileField", "desc"])
            },
            .. missile.GetTooltips()
        ];
    }

    public override Icon? GetIcon(State s)
    {
        return new Icon(ModEntry.Instance.Action_MissileField, null, Colors.textMain, false);
    }
}