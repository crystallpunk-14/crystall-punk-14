using Content.Shared._CP14.Religion.Components;
using Content.Shared._CP14.Religion.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.MagicSpell.Spells;

public sealed partial class CP14SpellGodTouch : CP14SpellEffect
{
    public override void Effect(EntityManager entManager, CP14SpellEffectBaseArgs args)
    {
        if (args.Target is null)
            return;

        if (!entManager.TryGetComponent<CP14ReligionEntityComponent>(args.User, out var god) || god.Religion is null)
            return;

        var ev = new CP14GodTouchEvent(god.Religion.Value);
        entManager.EventBus.RaiseLocalEvent(args.Target.Value, ev);
    }
}
public sealed class CP14GodTouchEvent(ProtoId<CP14ReligionPrototype> religion) : EntityEventArgs
{
    public ProtoId<CP14ReligionPrototype> Religion = religion;
}
