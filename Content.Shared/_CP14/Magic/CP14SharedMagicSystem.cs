using Content.Shared._CP14.Magic.Components;
using Content.Shared._CP14.Magic.Events;
using Content.Shared.DoAfter;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Physics.Systems;

namespace Content.Shared._CP14.Magic;

/// <summary>
///
/// </summary>
public sealed partial class CP14SharedMagicSystem : EntitySystem
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

        SubscribeLocalEvent<CP14DelayedInstantActionEvent>(OnInstantAction);
        SubscribeLocalEvent<CP14DelayedEntityTargetActionEvent>(OnEntityTargetAction);
        SubscribeLocalEvent<CP14DelayedWorldTargetActionEvent>(OnWorldTargetAction);

        InitializeSpells();
    }

    private void OnInstantAction(CP14DelayedInstantActionEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;

        if (args is not ICP14DelayedMagicEffect delayedEffect)
            return;

        var doAfterEventArgs = new DoAfterArgs(EntityManager, args.Performer, delayedEffect.Delay, new CP14DelayedInstantActionDoAfterEvent(), args.Action)
        {
            BreakOnMove = delayedEffect.BreakOnMove,
            BreakOnDamage = delayedEffect.BreakOnDamage,
        };

        _doAfter.TryStartDoAfter(doAfterEventArgs);
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

    private void OnBeforeCastMagicEffect(Entity<CP14MagicEffectComponent> ent, ref CP14BeforeCastMagicEffectEvent args)
    {
    }
}
