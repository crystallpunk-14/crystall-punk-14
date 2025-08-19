using Content.Shared._CP14.Vampire;
using Content.Shared._CP14.Vampire.Components;
using Content.Shared.FixedPoint;

namespace Content.Shared._CP14.MagicSpell.Spells;

public sealed partial class CP14SpellVampireGatherEssence : CP14SpellEffect
{
    [DataField]
    public FixedPoint2 Amount = 0.2f;

    public override void Effect(EntityManager entManager, CP14SpellEffectBaseArgs args)
    {
        if (args.Target is null)
            return;

        if (args.User is null)
            return;

        if (entManager.HasComponent<CP14VampireComponent>(args.Target.Value))
            return;

        if (!entManager.TryGetComponent<CP14VampireEssenceHolderComponent>(args.Target.Value, out var essenceHolder))
            return;

        var vamp = entManager.System<CP14SharedVampireSystem>();
        vamp.GatherEssence(args.User.Value, args.Target.Value, Amount);
    }
}
