using Content.Shared._CP14.MagicSpell.Components;
using Content.Shared._CP14.MagicSpell.Events;
using Content.Shared._CP14.MagicSpell.Spells;
using Content.Shared.DoAfter;
using Robust.Shared.Map;

namespace Content.Shared._CP14.MagicSpell;

public abstract partial class CP14SharedMagicSystem
{
    private void InitializeToggleableActions()
    {
        SubscribeLocalEvent<CP14ToggleableInstantActionEvent>(OnToggleableInstantAction);
        SubscribeLocalEvent<CP14ToggleableEntityWorldTargetActionEvent>(OnToggleableEntityWorldTargetAction);
        SubscribeLocalEvent<CP14ToggleableEntityTargetActionEvent>(OnToggleableEntityTargetAction);

        SubscribeLocalEvent<CP14MagicEffectComponent, CP14ToggleableInstantActionDoAfterEvent>(OnToggleableInstantActionDoAfterEvent);
        SubscribeLocalEvent<CP14MagicEffectComponent, CP14ToggleableEntityWorldTargetActionDoAfterEvent>(OnToggleableEntityWorldTargetActionDoAfterEvent);
        SubscribeLocalEvent<CP14MagicEffectComponent, CP14ToggleableEntityTargetActionDoAfterEvent>(OnToggleableEntityTargetActionDoAfterEvent);
    }

    private void UpdateToggleableActions()
    {
        var query = EntityQueryEnumerator<CP14MagicEffectComponent, CP14MagicEffectToggledComponent>();

        while (query.MoveNext(out var uid, out var effect, out var toggled))
        {
            if (toggled.NextTick > _timing.CurTime)
                continue;

            if (toggled.Performer is null)
                continue;

            toggled.NextTick = _timing.CurTime + TimeSpan.FromSeconds(toggled.Frequency);

            var spellArgs =
                new CP14SpellEffectBaseArgs(toggled.Performer, effect.SpellStorage, toggled.EntityTarget, toggled.WorldTarget);

            if (!CanCastSpell((uid, effect), spellArgs))
            {
                if (_doAfter.IsRunning(toggled.DoAfterId))
                    _doAfter.Cancel(toggled.DoAfterId);
                continue;
            }

            CastSpell((uid, effect), spellArgs);
        }
    }

    private void OnToggleableInstantActionDoAfterEvent(Entity<CP14MagicEffectComponent> ent, ref CP14ToggleableInstantActionDoAfterEvent args)
    {
        EndToggleableAction(ent, args.User, args.Cooldown);
    }

    private void OnToggleableEntityWorldTargetActionDoAfterEvent(Entity<CP14MagicEffectComponent> ent, ref CP14ToggleableEntityWorldTargetActionDoAfterEvent args)
    {
        EndToggleableAction(ent, args.User, args.Cooldown);
    }

    private void OnToggleableEntityTargetActionDoAfterEvent(Entity<CP14MagicEffectComponent> ent, ref CP14ToggleableEntityTargetActionDoAfterEvent args)
    {
        EndToggleableAction(ent, args.User, args.Cooldown);
    }

    private void StartToggleableAction(ICP14ToggleableMagicEffect toggleable, DoAfterEvent doAfter, Entity<CP14MagicEffectComponent> action, EntityUid performer, EntityUid? entityTarget = null, EntityCoordinates? worldTarget = null)
    {
        if (_doAfter.IsRunning(action.Comp.ActiveDoAfter))
            return;

        // event may return an empty entity with id = 0, which causes bugs
        var _target = entityTarget;
        if (_target is not null && _target.Value.Id == 0)
            _target = null;

        var evStart = new CP14StartCastMagicEffectEvent(performer);
        RaiseLocalEvent(action, ref evStart);

        var fromItem = action.Comp.SpellStorage is not null && action.Comp.SpellStorage.Value != performer;

        var doAfterEventArgs = new DoAfterArgs(EntityManager, performer, toggleable.CastTime, doAfter, action, used: action.Comp.SpellStorage, target: _target)
        {
            BreakOnMove = toggleable.BreakOnMove,
            BreakOnDamage = toggleable.BreakOnDamage,
            Hidden = toggleable.Hidden,
            DistanceThreshold = toggleable.DistanceThreshold,
            CancelDuplicate = true,
            BlockDuplicate = true,
            BreakOnDropItem = fromItem,
            NeedHand = fromItem,
        };

        if (_doAfter.TryStartDoAfter(doAfterEventArgs, out var doAfterId))
        {
            EnsureComp<CP14MagicEffectToggledComponent>(action, out var toggled);
            toggled.Frequency = toggleable.EffectFrequency;
            toggled.Performer = performer;
            toggled.DoAfterId = doAfterId;
            toggled.Cooldown = toggleable.Cooldown;

            toggled.EntityTarget = _target;
            toggled.WorldTarget = worldTarget;

            action.Comp.ActiveDoAfter = doAfterId;
        }
    }

    private void EndToggleableAction(Entity<CP14MagicEffectComponent> action, EntityUid performer, float? cooldown = null)
    {
        if (cooldown is not null)
            _action.CP14StartCustomDelay(action, TimeSpan.FromSeconds(cooldown.Value));
        RemCompDeferred<CP14MagicEffectToggledComponent>(action);

        var endEv = new CP14EndCastMagicEffectEvent(performer);
        RaiseLocalEvent(action, ref endEv);
    }

    private void ToggleToggleableAction(ICP14ToggleableMagicEffect toggleable, DoAfterEvent doAfter, Entity<CP14MagicEffectComponent> action, EntityUid performer, EntityUid? entityTarget = null, EntityCoordinates? worldTarget = null)
    {
        var spellArgs = new CP14SpellEffectBaseArgs(performer, entityTarget, entityTarget, worldTarget);
        if (!CanCastSpell(action, spellArgs))
            return;

        if (_doAfter.IsRunning(action.Comp.ActiveDoAfter))
            _doAfter.Cancel(action.Comp.ActiveDoAfter);
        else
            StartToggleableAction(toggleable, doAfter, action, performer, entityTarget, worldTarget);
    }

    /// <summary>
    /// Instant action used from hotkey event
    /// </summary>
    private void OnToggleableInstantAction(CP14ToggleableInstantActionEvent args)
    {
        if (args.Handled)
            return;

        if (args is not ICP14ToggleableMagicEffect toggleable)
            return;

        if (!TryComp<CP14MagicEffectComponent>(args.Action, out var magicEffect))
            return;

        var doAfter = new CP14ToggleableInstantActionDoAfterEvent(args.Cooldown);
        ToggleToggleableAction(toggleable, doAfter, (args.Action, magicEffect), args.Performer, args.Performer, Transform(args.Performer).Coordinates);

        args.Handled = true;
    }

    /// <summary>
    /// Target action used from hotkey event
    /// </summary>
    private void OnToggleableEntityWorldTargetAction(CP14ToggleableEntityWorldTargetActionEvent args)
    {
        if (args.Handled)
            return;

        if (args is not ICP14ToggleableMagicEffect toggleable)
            return;

        if (!TryComp<CP14MagicEffectComponent>(args.Action, out var magicEffect))
            return;

        var doAfter = new CP14ToggleableEntityWorldTargetActionDoAfterEvent(
            EntityManager.GetNetCoordinates(args.Coords),
            EntityManager.GetNetEntity(args.Entity),
            args.Cooldown);
        ToggleToggleableAction(toggleable, doAfter, (args.Action, magicEffect), args.Performer, args.Entity, args.Coords);

        args.Handled = true;
    }

    /// <summary>
    /// Entity target action used from hotkey event
    /// </summary>
    private void OnToggleableEntityTargetAction(CP14ToggleableEntityTargetActionEvent args)
    {
        if (args.Handled)
            return;

        if (args is not ICP14ToggleableMagicEffect toggleable)
            return;

        if (!TryComp<CP14MagicEffectComponent>(args.Action, out var magicEffect))
            return;

        var doAfter = new CP14ToggleableEntityTargetActionDoAfterEvent(EntityManager.GetNetEntity(args.Target), args.Cooldown);
        ToggleToggleableAction(toggleable, doAfter, (args.Action, magicEffect), args.Performer, args.Target, Transform(args.Target).Coordinates);

        args.Handled = true;
    }
}

