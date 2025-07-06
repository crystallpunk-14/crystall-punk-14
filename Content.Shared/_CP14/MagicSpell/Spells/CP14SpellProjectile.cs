using System.Numerics;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Map;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Shared._CP14.MagicSpell.Spells;

public sealed partial class CP14SpellProjectile : CP14SpellEffect
{
    [DataField(required: true)]
    public EntProtoId Prototype;

    [DataField]
    public float ProjectileSpeed = 20f;

    [DataField]
    public float Spread = 0f;

    [DataField]
    public int ProjectileCount = 1;

    [DataField]
    public bool SaveVelocity = false;

    public override void Effect(EntityManager entManager, CP14SpellEffectBaseArgs args)
    {
        EntityCoordinates? targetPoint = null;

        if (args.Target is not null &&
            entManager.TryGetComponent<TransformComponent>(args.Target.Value, out var transformComponent))
            targetPoint = transformComponent.Coordinates;
        else if (args.Position is not null)
            targetPoint = args.Position;

        if (targetPoint is null)
            return;

        var transform = entManager.System<SharedTransformSystem>();
        var physics = entManager.System<SharedPhysicsSystem>();
        var gunSystem = entManager.System<SharedGunSystem>();
        var mapManager = IoCManager.Resolve<IMapManager>();
        var random = IoCManager.Resolve<IRobustRandom>();

        if (!entManager.TryGetComponent<TransformComponent>(args.User, out var xform))
            return;

        var fromCoords = xform.Coordinates;


        var userVelocity = physics.GetMapLinearVelocity(args.User.Value);

        // If applicable, this ensures the projectile is parented to grid on spawn, instead of the map.
        var fromMap = transform.ToMapCoordinates(fromCoords);

        var spawnCoords = mapManager.TryFindGridAt(fromMap, out var gridUid, out _)
            ? transform.WithEntityId(fromCoords, gridUid)
            : new(mapManager.GetMapEntityId(fromMap.MapId), fromMap.Position);

        for (var i = 0; i < ProjectileCount; i++)
        {
            //Apply spread to target point
            var offsetedTargetPoint = targetPoint.Value.Offset(new Vector2(
                (float) (random.NextDouble() * 2 - 1) * Spread,
                (float) (random.NextDouble() * 2 - 1) * Spread));

            if (fromCoords == offsetedTargetPoint)
                continue;

            var ent = entManager.PredictedSpawnAtPosition(Prototype, spawnCoords);

            var direction = offsetedTargetPoint.ToMapPos(entManager, transform) -
                            spawnCoords.ToMapPos(entManager, transform);

            gunSystem.ShootProjectile(ent, direction, SaveVelocity ? userVelocity : new Vector2(), args.User.Value, args.User, ProjectileSpeed);
        }
    }
}
