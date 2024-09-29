using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Map;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.MagicSpell.Spells;

public sealed partial class CP14SpellProjectile : CP14SpellEffect
{
    [DataField(required: true)]
    public EntProtoId Prototype;

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

        if (!entManager.TryGetComponent<TransformComponent>(args.User, out var xform))
            return;

        var fromCoords = xform.Coordinates;

        if (fromCoords == targetPoint)
            return;

        var userVelocity = physics.GetMapLinearVelocity(args.User.Value);

        // If applicable, this ensures the projectile is parented to grid on spawn, instead of the map.
        var fromMap = transform.ToMapCoordinates(fromCoords);
        var spawnCoords = mapManager.TryFindGridAt(fromMap, out var gridUid, out _)
            ? transform.WithEntityId(fromCoords, gridUid)
            : new(mapManager.GetMapEntityId(fromMap.MapId), fromMap.Position);


        var ent = entManager.SpawnAtPosition(Prototype, spawnCoords);
        var direction = targetPoint.Value.ToMapPos(entManager, transform) -
                        spawnCoords.ToMapPos(entManager, transform);
        gunSystem.ShootProjectile(ent, direction, userVelocity, args.User.Value, args.User);
    }
}
