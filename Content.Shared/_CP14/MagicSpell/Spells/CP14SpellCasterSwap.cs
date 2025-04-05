using Content.Shared.Examine;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;

namespace Content.Shared._CP14.MagicSpell.Spells;

public sealed partial class CP14SpellCasterSwap : CP14SpellEffect
{
    [DataField]
    public bool NeedVision = true;

    [DataField]
    public bool OnlyAlive = true;

    public override void Effect(EntityManager entManager, CP14SpellEffectBaseArgs args)
    {
        if (args.User is not { } user || args.Target is not { } target)
            return;

        var transform = entManager.System<SharedTransformSystem>();
        var userPosition = transform.GetMoverCoordinates(user);
        var targetPosition = transform.GetMoverCoordinates(target);

        transform.SetCoordinates(user, targetPosition);
        transform.SetCoordinates(target, userPosition);
    }

    public override bool CanCast(EntityManager entManager, CP14SpellEffectBaseArgs args)
    {
        if (args.User is not { } user || args.Target is not { } target)
            return false;

        var transform = entManager.System<SharedTransformSystem>();
        var targetPosition = transform.GetMoverCoordinates(target);

        var mobState = entManager.System<MobStateSystem>();
        if (OnlyAlive)
        {
            if (!entManager.TryGetComponent<MobStateComponent>(target, out var targetMobStateComp))
                return false;

            if (mobState.IsDead(target, targetMobStateComp))
                return false;
        }

        var examine = entManager.System<ExamineSystemShared>();
        if (NeedVision && !examine.InRangeUnOccluded(user, targetPosition))
            return false;

        return true;
    }
}
