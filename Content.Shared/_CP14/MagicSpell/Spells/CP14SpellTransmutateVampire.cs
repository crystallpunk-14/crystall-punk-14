using Content.Shared._CP14.Transmutation;
using Content.Shared._CP14.Vampire;

namespace Content.Shared._CP14.MagicSpell.Spells;

public sealed partial class CP14SpellTransmutateVampire : CP14SpellEffect
{
    public override void Effect(EntityManager entManager, CP14SpellEffectBaseArgs args)
    {
        if (args.User is null)
            return;

        if (args.Target is null)
            return;

        if (!entManager.TryGetComponent<CP14VampireComponent>(args.User.Value, out var vampireComponent))
            return;

        if (vampireComponent.TransmutationMethod is null)
            return;

        var transmutateSys = entManager.System<CP14TransmutationSystem>();

        transmutateSys.TryTransmutate(args.Target.Value, vampireComponent.TransmutationMethod.Value);
    }
}
