using Content.Shared._CP14.Actions;
using Content.Shared.Actions.Components;
using Content.Shared.Actions.Events;
using Content.Shared.DoAfter;
using Content.Shared.Item;

namespace Content.Shared.Actions;

public abstract partial class SharedActionsSystem
{
    protected void InitializeActionDoAfter()
    {
        SubscribeLocalEvent<DoAfterArgsComponent, ActionDoAfterEvent>(OnActionDoAfter);
    }

    private bool TryStartActionDoAfter(Entity<DoAfterArgsComponent> ent, Entity<DoAfterComponent?> performer, TimeSpan? originalUseDelay, RequestPerformActionEvent input)
    {
        // relay to user
        if (!Resolve(performer, ref performer.Comp))
            return false;

        var delay = ent.Comp.Delay;

        var netEnt = GetNetEntity(performer);

        //CP14 doAfter start event
        var target = GetEntity(input.EntityTarget);
        EntityUid? used = null;

        if (TryComp<ActionComponent>(ent, out var action) && HasComp<ItemComponent>(action.Container))
        {
            used = action.Container;
        }

        var cp14StartEv = new CP14ActionStartDoAfterEvent(netEnt, input);
        RaiseLocalEvent(ent, cp14StartEv);
        //CP14 end

        var actionDoAfterEvent = new ActionDoAfterEvent(netEnt, originalUseDelay, input);

        var doAfterArgs = new DoAfterArgs(EntityManager, performer, delay, actionDoAfterEvent, ent.Owner, target ?? performer, used) //CP14 edited target and added used
        {
            AttemptFrequency = ent.Comp.AttemptFrequency,
            Broadcast = ent.Comp.Broadcast,
            Hidden = ent.Comp.Hidden,
            NeedHand = ent.Comp.NeedHand,
            BreakOnHandChange = ent.Comp.BreakOnHandChange,
            BreakOnDropItem = ent.Comp.BreakOnDropItem,
            BreakOnMove = ent.Comp.BreakOnMove,
            BreakOnWeightlessMove = ent.Comp.BreakOnWeightlessMove,
            MovementThreshold = ent.Comp.MovementThreshold,
            DistanceThreshold = ent.Comp.DistanceThreshold,
            BreakOnDamage = ent.Comp.BreakOnDamage,
            DamageThreshold = ent.Comp.DamageThreshold,
            RequireCanInteract = ent.Comp.RequireCanInteract
        };

        return _doAfter.TryStartDoAfter(doAfterArgs, performer);
    }

    private void OnActionDoAfter(Entity<DoAfterArgsComponent> ent, ref ActionDoAfterEvent args)
    {
        if (!_actionQuery.TryComp(ent, out var actionComp))
            return;

        var performer = GetEntity(args.Performer);
        var action = (ent, actionComp);

        // If this doafter is on repeat and was cancelled, start use delay as expected
        if (args.Cancelled && ent.Comp.Repeat)
        {
            SetUseDelay(action, args.OriginalUseDelay);
            RemoveCooldown(action);
            StartUseDelay(action);
            UpdateAction(action);
            return;
        }

        args.Repeat = ent.Comp.Repeat;

        // Set the use delay to 0 so this can repeat properly
        if (ent.Comp.Repeat)
        {
            SetUseDelay(action, TimeSpan.Zero);
        }

        //CP14 start delay after cancelling for preventing spamming
        if (args.Cancelled)
            StartUseDelay(action);
        //CP14 end

        if (args.Cancelled)
            return;

        // Post original doafter, reduce the time on it now for other casts if ables
        if (ent.Comp.DelayReduction != null)
            args.Args.Delay = ent.Comp.DelayReduction.Value;

        // Validate again for charges, blockers, etc
        if (TryPerformAction(args.Input, performer, skipDoActionRequest: true))
            return;

        // Cancel this doafter if we can't validate the action
        _doAfter.Cancel(args.DoAfter.Id, force: true);
    }
}
