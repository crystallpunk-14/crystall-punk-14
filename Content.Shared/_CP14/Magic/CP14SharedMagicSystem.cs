using Content.Shared._CP14.Magic.Components;
using Content.Shared._CP14.Magic.Events;
using Content.Shared.DoAfter;
using Content.Shared.EntityEffects;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Physics.Systems;

namespace Content.Shared._CP14.Magic;

/// <summary>
///
/// </summary>
public sealed class CP14SharedMagicSystem : EntitySystem
{
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly SharedGunSystem _gunSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14MagicEffectComponent, CP14BeforeCastMagicEffectEvent>(OnBeforeCastMagicEffect);

        SubscribeLocalEvent<CP14DelayedEntityTargetActionEvent>(OnEntityTargetAction);
        SubscribeLocalEvent<CP14DelayedEntityTargetActionComponent, CP14DelayedEntityTargetActionDoAfterEvent>(OnCastEntityTarget);

        SubscribeLocalEvent<CP14DelayedWorldTargetActionEvent>(OnWorldTargetAction);
        SubscribeLocalEvent<CP14DelayedProjectileSpellComponent, CP14DelayedWorldTargetActionDoAfterEvent>(OnCastProjectileSpell);
    }

    private void OnWorldTargetAction(CP14DelayedWorldTargetActionEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;

        if (args is not ICP14DelayedMagicEffect delayedEffect)
            return;

        var doAfter = new CP14DelayedWorldTargetActionDoAfterEvent()
        {
            Target = EntityManager.GetNetCoordinates(args.Target)
        };

        var doAfterEventArgs = new DoAfterArgs(EntityManager, args.Performer, delayedEffect.Delay, doAfter, args.Action)
        {
            BreakOnMove = delayedEffect.BreakOnMove,
            BreakOnDamage = delayedEffect.BreakOnDamage,
        };

        _doAfter.TryStartDoAfter(doAfterEventArgs);
    }

    private void OnEntityTargetAction(CP14DelayedEntityTargetActionEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;

        if (args is not ICP14DelayedMagicEffect delayedEffect)
            return;

        var doAfterEventArgs = new DoAfterArgs(EntityManager, args.Performer, delayedEffect.Delay, new CP14DelayedEntityTargetActionDoAfterEvent(), args.Action, args.Target)
        {
            BreakOnMove = delayedEffect.BreakOnMove,
            BreakOnDamage = delayedEffect.BreakOnDamage,
        };

        _doAfter.TryStartDoAfter(doAfterEventArgs);
    }

    private void OnCastEntityTarget(Entity<CP14DelayedEntityTargetActionComponent> ent, ref CP14DelayedEntityTargetActionDoAfterEvent args)
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


    private void OnBeforeCastMagicEffect(Entity<CP14MagicEffectComponent> ent, ref CP14BeforeCastMagicEffectEvent args)
    {
    }
}
