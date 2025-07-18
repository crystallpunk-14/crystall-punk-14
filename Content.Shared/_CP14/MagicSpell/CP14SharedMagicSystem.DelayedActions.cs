using Content.Shared._CP14.MagicSpell.Components;
using Content.Shared._CP14.MagicSpell.Events;
using Content.Shared._CP14.MagicSpell.Spells;
using Content.Shared.DoAfter;
using Robust.Shared.Map;

namespace Content.Shared._CP14.MagicSpell;

public abstract partial class CP14SharedMagicSystem
{
    private void InitializeDelayedActions()
    {
        SubscribeLocalEvent<CP14DelayedInstantActionEvent>(OnInstantAction);
        SubscribeLocalEvent<CP14DelayedWorldTargetActionEvent>(OnWorldTargetAction);
        SubscribeLocalEvent<CP14DelayedEntityTargetActionEvent>(OnEntityTargetAction);

        SubscribeLocalEvent<CP14MagicEffectComponent, CP14DelayedInstantActionDoAfterEvent>(OnDelayedInstantActionDoAfter);
        SubscribeLocalEvent<CP14MagicEffectComponent, CP14DelayedEntityWorldTargetActionDoAfterEvent>(OnDelayedEntityWorldTargetDoAfter);
        SubscribeLocalEvent<CP14MagicEffectComponent, CP14DelayedEntityTargetActionDoAfterEvent>(OnDelayedEntityTargetDoAfter);
    }

    private bool TryStartDelayedAction(ICP14DelayedMagicEffect delayedEffect, DoAfterEvent doAfter, Entity<CP14MagicEffectComponent> action, EntityUid? target, EntityUid performer)
    {
        if (_doAfter.IsRunning(action.Comp.ActiveDoAfter))
            return false;

        var fromItem = action.Comp.SpellStorage is not null && action.Comp.SpellStorage.Value != performer;

        var doAfterEventArgs = new DoAfterArgs(EntityManager, performer, MathF.Max(delayedEffect.CastDelay, 0.3f), doAfter, action, used: action.Comp.SpellStorage, target: target)
        {
            BreakOnMove = delayedEffect.BreakOnMove,
            BreakOnDamage = delayedEffect.BreakOnDamage,
            Hidden = delayedEffect.Hidden,
            DistanceThreshold = delayedEffect.DistanceThreshold,
            CancelDuplicate = true,
            BlockDuplicate = true,
            BreakOnDropItem = fromItem,
            NeedHand = fromItem,
            RequireCanInteract = delayedEffect.RequireCanInteract,
        };

        if (_doAfter.TryStartDoAfter(doAfterEventArgs, out var doAfterId))
        {
            action.Comp.ActiveDoAfter = doAfterId;
            return true;
        }

        return false;
    }

    private void EndDelayedAction(Entity<CP14MagicEffectComponent> action, EntityUid performer, float? cooldown = null)
    {
        var endEv = new CP14EndCastMagicEffectEvent(performer);
        RaiseLocalEvent(action, ref endEv);

        if (cooldown is not null)
            _action.SetCooldown(action.Owner, TimeSpan.FromSeconds(cooldown.Value));
    }

    private bool UseDelayedAction(ICP14DelayedMagicEffect delayedEffect, Entity<CP14MagicEffectComponent> action, DoAfterEvent doAfter, EntityUid performer, EntityUid? target = null, EntityCoordinates? worldTarget = null)
    {
        // Event may return an empty entity with id = 0, which causes bugs
        var currentTarget = target;
        if (currentTarget is not null && currentTarget == EntityUid.Invalid)
            currentTarget = null;

        var spellArgs = new CP14SpellEffectBaseArgs(performer, action.Comp.SpellStorage, currentTarget, worldTarget);
        if (!CanCastSpell(action, spellArgs))
            return false;

        if (_doAfter.IsRunning(action.Comp.ActiveDoAfter))
        {
            _doAfter.Cancel(action.Comp.ActiveDoAfter);
            return false;
        }

        if (!TryStartDelayedAction(delayedEffect, doAfter, action, currentTarget, performer))
            return false;

        var evStart = new CP14StartCastMagicEffectEvent(performer);
        RaiseLocalEvent(action, ref evStart);
        CastTelegraphy(action, spellArgs);
        return true;
    }

    /// <summary>
    /// Instant action used from hotkey event
    /// </summary>
    private void OnInstantAction(CP14DelayedInstantActionEvent args)
    {
        if (args.Handled)
            return;

        if (args is not ICP14DelayedMagicEffect delayedEffect)
            return;

        if (!TryComp<CP14MagicEffectComponent>(args.Action, out var magicEffect))
            return;

        var doAfter = new CP14DelayedInstantActionDoAfterEvent(args.Cooldown);

        if (!UseDelayedAction(delayedEffect, (args.Action, magicEffect), doAfter, args.Performer, args.Performer))
            return;

        args.Handled = true;
    }

    /// <summary>
    /// Target action used from hotkey event
    /// </summary>
    private void OnWorldTargetAction(CP14DelayedWorldTargetActionEvent args)
    {
        if (args.Handled)
            return;

        if (args is not ICP14DelayedMagicEffect delayedEffect)
            return;

        if (!TryComp<CP14MagicEffectComponent>(args.Action, out var magicEffect))
            return;

        var doAfter = new CP14DelayedEntityWorldTargetActionDoAfterEvent(
            EntityManager.GetNetCoordinates(args.Target),
            EntityManager.GetNetEntity(args.Entity),
            args.Cooldown);

        if (!UseDelayedAction(delayedEffect, (args.Action, magicEffect), doAfter, args.Performer, args.Entity, args.Target))
            return;

        args.Handled = true;
    }

    /// <summary>
    /// Target action used from hotkey event
    /// </summary>
    private void OnEntityTargetAction(CP14DelayedEntityTargetActionEvent args)
    {
        if (args.Handled)
            return;

        if (args is not ICP14DelayedMagicEffect delayedEffect)
            return;

        if (!TryComp<CP14MagicEffectComponent>(args.Action, out var magicEffect))
            return;

        var doAfter = new CP14DelayedEntityTargetActionDoAfterEvent(EntityManager.GetNetEntity(args.Target), args.Cooldown);

        if (!UseDelayedAction(delayedEffect, (args.Action, magicEffect), doAfter, args.Performer, args.Target))
            return;

        args.Handled = true;
    }

    /// <summary>
    /// Action doAfter finished or interrupted
    /// </summary>
    private void OnDelayedInstantActionDoAfter(Entity<CP14MagicEffectComponent> ent, ref CP14DelayedInstantActionDoAfterEvent args)
    {
        EndDelayedAction(ent, args.User, args.Cooldown);

        if (args.Cancelled || args.Handled)
            return;

        CastSpell(ent, new CP14SpellEffectBaseArgs(args.User, args.Used, args.User, Transform(args.User).Coordinates));

        args.Handled = true;
    }

    /// <summary>
    /// Action doAfter finished or interrupted
    /// </summary>
    private void OnDelayedEntityWorldTargetDoAfter(Entity<CP14MagicEffectComponent> ent,
        ref CP14DelayedEntityWorldTargetActionDoAfterEvent args)
    {
        EndDelayedAction(ent, args.User, args.Cooldown);

        if (args.Cancelled || args.Handled)
            return;

        var targetPos = EntityManager.GetCoordinates(args.TargetPosition);
        EntityManager.TryGetEntity(args.TargetEntity, out var targetEnt);

        CastSpell(ent, new CP14SpellEffectBaseArgs(args.User, args.Used, targetEnt, targetPos));

        args.Handled = true;
    }

    /// <summary>
    /// Action doAfter finished or interrupted
    /// </summary>
    private void OnDelayedEntityTargetDoAfter(Entity<CP14MagicEffectComponent> ent, ref CP14DelayedEntityTargetActionDoAfterEvent args)
    {
        EndDelayedAction(ent, args.User, args.Cooldown);

        if (args.Cancelled || args.Handled)
            return;

        EntityManager.TryGetEntity(args.TargetEntity, out var targetEnt);
        EntityCoordinates? targetPos = null;
        if (targetEnt is not null) { targetPos = Transform(targetEnt.Value).Coordinates; }

        CastSpell(ent, new CP14SpellEffectBaseArgs(args.User, args.Used, targetEnt, targetPos));

        args.Handled = true;
    }
}
