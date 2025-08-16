using Content.Shared._CP14.Transmutation;
using Content.Shared._CP14.Transmutation.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.MagicSpell.Spells;

public sealed partial class CP14SpellTransmutate : CP14SpellEffect
{
    [DataField(required: true)]
    public ProtoId<CP14TransmutationPrototype> Method;

    public override void Effect(EntityManager entManager, CP14SpellEffectBaseArgs args)
    {
        if (args.Target is null)
            return;

        var transmutateSys = entManager.System<CP14TransmutationSystem>();

        transmutateSys.TryTransmutate(args.Target.Value, Method);
    }
}
