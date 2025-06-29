using Content.Shared.Projectiles;
using Content.Shared.Throwing;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.MagicSpell.Spells;

public sealed partial class CP14SpellSpawnEntityOnUserAndThrowTo : CP14SpellEffect
{
    [DataField]
    public List<EntProtoId> Spawns = new();

    [DataField]
    public float ThrowPower = 10f;

    public override void Effect(EntityManager entManager, CP14SpellEffectBaseArgs args)
    {
        if (args.User is null || !entManager.TryGetComponent<TransformComponent>(args.User.Value, out var userTransform) )
            return;

        EntityCoordinates targetPoint;

        if (args.Target is not null &&
            entManager.TryGetComponent<TransformComponent>(args.Target.Value, out var targetTransform))
            targetPoint = targetTransform.Coordinates;
        else if (args.Position is not null)
            targetPoint = (EntityCoordinates) args.Position;
        else
            return;

        var throwing = entManager.System<ThrowingSystem>();
        // var xform = entManager.System<SharedTransformSystem>();

        // var worldPos = xform.GetWorldPosition(args.User.Value);
        // var foo = xform.GetWorldPosition(args.Target.Value) - worldPos;

        foreach (var spawn in Spawns)
        {
            EntityUid spawnedEntity = entManager.SpawnAtPosition(spawn, userTransform.Coordinates);

            throwing.TryThrow(spawnedEntity, targetPoint, ThrowPower, args.User, doSpin: true);
        }

    }
}
