using Content.Shared._CP14.Religion.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.MagicSpell.Spells;

public sealed partial class CP14SpellGodTouch : CP14SpellEffect
{
    [DataField]
    public ProtoId<CP14ReligionPrototype> Religion = default!;
    public override void Effect(EntityManager entManager, CP14SpellEffectBaseArgs args)
    {
        if (args.Target is null)
            return;

        var ev = new CP14GodTouchEvent(Religion);
        entManager.EventBus.RaiseLocalEvent(args.Target.Value, ev);
    }
}
public sealed class CP14GodTouchEvent(ProtoId<CP14ReligionPrototype> religion) : EntityEventArgs
{
    public ProtoId<CP14ReligionPrototype> Religion = religion;
}
