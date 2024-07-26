
using Content.Shared._CP14.Magic.Components.Spells;
using Content.Shared._CP14.Magic.Events;
using Content.Shared.EntityEffects;

namespace Content.Shared._CP14.Magic;

public sealed partial class CP14SharedMagicSystem
{
    private void InitializeSpells()
    {
        // Instants
        SubscribeLocalEvent<CP14DelayedSpawnEntitiesSpellComponent, CP14DelayedInstantActionDoAfterEvent>(OnCastEntitiesSpawn);
        SubscribeLocalEvent<CP14DelayedSelfEntityEffectSpellComponent, CP14DelayedInstantActionDoAfterEvent>(OnCastSelfEntityEffects);
        //Entity Target
        SubscribeLocalEvent<CP14DelayedApplyEntityEffectsSpellComponent, CP14DelayedEntityTargetActionDoAfterEvent>(OnCastApplyEntityEffects);
        //World Target
        SubscribeLocalEvent<CP14DelayedProjectileSpellComponent, CP14DelayedWorldTargetActionDoAfterEvent>(OnCastProjectileSpell);
        SubscribeLocalEvent<CP14DelayedSpawnOnWorldTargetSpellComponent, CP14DelayedWorldTargetActionDoAfterEvent>(OnCastSpawnOnPoint);
    }

    private void OnCastEntitiesSpawn(Entity<CP14DelayedSpawnEntitiesSpellComponent> ent, ref CP14DelayedInstantActionDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || !_net.IsServer)
            return;

        args.Handled = true;

        foreach (var spawn in ent.Comp.Spawns)
        {
            SpawnAtPosition(spawn, Transform(args.User).Coordinates);
        }
    }

    private void OnCastSelfEntityEffects(Entity<CP14DelayedSelfEntityEffectSpellComponent> ent, ref CP14DelayedInstantActionDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        args.Handled = true;

        foreach (var effect in ent.Comp.Effects)
        {
            effect.Effect(new EntityEffectBaseArgs(args.User, EntityManager));
        }
    }

    private void OnCastApplyEntityEffects(Entity<CP14DelayedApplyEntityEffectsSpellComponent> ent, ref CP14DelayedEntityTargetActionDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || args.Target == null)
            return;

        args.Handled = true;

        foreach (var effect in ent.Comp.Effects)
        {
            effect.Effect(new EntityEffectBaseArgs(args.Target.Value, EntityManager));
        }
    }

    private void OnCastProjectileSpell(Entity<CP14DelayedProjectileSpellComponent> spell, ref CP14DelayedWorldTargetActionDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || !_net.IsServer)
            return;

        args.Handled = true;

        var xform = Transform(args.User);
        var fromCoords = xform.Coordinates;
        var toCoords = GetCoordinates(args.Target);
        var userVelocity = _physics.GetMapLinearVelocity(args.User);

        // If applicable, this ensures the projectile is parented to grid on spawn, instead of the map.
        var fromMap = fromCoords.ToMap(EntityManager, _transform);
        var spawnCoords = _mapManager.TryFindGridAt(fromMap, out var gridUid, out _)
            ? fromCoords.WithEntityId(gridUid, EntityManager)
            : new(_mapManager.GetMapEntityId(fromMap.MapId), fromMap.Position);

        var ent = Spawn(spell.Comp.Prototype, spawnCoords);
        var direction = toCoords.ToMapPos(EntityManager, _transform) -
                        spawnCoords.ToMapPos(EntityManager, _transform);
        _gunSystem.ShootProjectile(ent, direction, userVelocity, args.User, args.User);
    }

    private void OnCastSpawnOnPoint(Entity<CP14DelayedSpawnOnWorldTargetSpellComponent> ent, ref CP14DelayedWorldTargetActionDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || !_net.IsServer)
            return;

        args.Handled = true;

        var xform = Transform(args.User);
        var toCoords = GetCoordinates(args.Target);

        foreach (var spawn in ent.Comp.Spawns)
        {
            SpawnAtPosition(spawn, toCoords);
        }
    }
}
