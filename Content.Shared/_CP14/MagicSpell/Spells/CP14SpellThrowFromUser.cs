using Content.Shared.Throwing;

namespace Content.Shared._CP14.MagicSpell.Spells;

public sealed partial class CP14SpellThrowFromUser : CP14SpellEffect
{
    [DataField]
    public float ThrowPower = 10f;

    public override void Effect(EntityManager entManager, CP14SpellEffectBaseArgs args)
    {
        if (args.Target is null)
            return;

        var targetEntity = args.Target.Value;

        var throwing = entManager.System<ThrowingSystem>();
        var xfom = entManager.System<SharedTransformSystem>();

        if (!entManager.TryGetComponent<TransformComponent>(args.User, out var userTransform))
            return;

        if (!entManager.TryGetComponent<TransformComponent>(targetEntity, out var targetTransform))
            return;

        var worldPos = xfom.GetWorldPosition(args.User.Value);
        var foo = xfom.GetWorldPosition(args.Target.Value) - worldPos;

        throwing.TryThrow(targetEntity, foo * 2.5f, ThrowPower, args.User, doSpin: true);
    }
}
