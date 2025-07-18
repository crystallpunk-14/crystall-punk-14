using System.Numerics;
using Content.Shared.Projectiles;
using Content.Shared.Throwing;

namespace Content.Shared._CP14.MagicSpell.Spells;

public sealed partial class CP14SpellThrowFromUser : CP14SpellEffect
{
    [DataField]
    public float ThrowPower = 10f;

    [DataField]
    public float Distance = 2.5f;

    public override void Effect(EntityManager entManager, CP14SpellEffectBaseArgs args)
    {
        if (args.Target is null || args.User is null)
            return;

        var targetEntity = args.Target.Value;

        var throwing = entManager.System<ThrowingSystem>();
        var xform = entManager.System<SharedTransformSystem>();

        var worldPos = xform.GetWorldPosition(args.User.Value);
        var foo = Vector2.Normalize(xform.GetWorldPosition(args.Target.Value) - worldPos);

        if (entManager.TryGetComponent<EmbeddableProjectileComponent>(targetEntity, out var embeddable))
        {
            var projectile = entManager.System<SharedProjectileSystem>();

            projectile.EmbedDetach(targetEntity, embeddable);
        }

        throwing.TryThrow(targetEntity, foo * Distance, ThrowPower, args.User, doSpin: true);
    }
}
