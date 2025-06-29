using Content.Shared.Projectiles;
using Content.Shared.Throwing;
using Content.Shared._CP14.MagicSpell.Components;

namespace Content.Shared._CP14.MagicSpell.Spells;

public sealed partial class CP14SpellThrowFromUser : CP14SpellEffect
{
    [DataField]
    public float ThrowPower = 10f;

    [DataField]
    public bool IsWall = false;

    public override void Effect(EntityManager entManager, CP14SpellEffectBaseArgs args)
    {
        if (args.Target is null)
            return;

        var targetEntity = args.Target.Value;

        var throwing = entManager.System<ThrowingSystem>();
        var _transform = entManager.System<SharedTransformSystem>();

        if (!entManager.TryGetComponent<TransformComponent>(args.User, out var userTransform))
            return;

        if (!entManager.TryGetComponent<TransformComponent>(targetEntity, out var targetTransform))
            return;

        var worldPos = _transform.GetWorldPosition(args.User.Value);
        var foo = _transform.GetWorldPosition(args.Target.Value) - worldPos;

        if (entManager.TryGetComponent<EmbeddableProjectileComponent>(targetEntity, out var embeddable))
        {
            var projectile = entManager.System<SharedProjectileSystem>();

            projectile.EmbedDetach(targetEntity, embeddable);
        }

        if (IsWall)
        {
            var xform = entManager.GetComponent<TransformComponent>(targetEntity);
            _transform.Unanchor(targetEntity, xform);
        }

        throwing.TryThrow(targetEntity, foo * 2.5f, ThrowPower, args.User, doSpin: true);
    }
}
