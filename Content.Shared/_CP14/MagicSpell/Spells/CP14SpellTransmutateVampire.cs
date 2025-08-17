using Content.Shared._CP14.Transmutation;
using Content.Shared._CP14.Vampire;
using Content.Shared._CP14.Vampire.Components;
using Robust.Shared.Prototypes;

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

        var protoMan = IoCManager.Resolve<IPrototypeManager>();

        var method = protoMan.Index(vampireComponent.Faction).TransmutationMethod;
        if (method is null)
            return;

        var transmutateSys = entManager.System<CP14TransmutationSystem>();

        transmutateSys.TryTransmutate(args.Target.Value, method.Value, args.User.Value);
    }
}
