using Content.Shared._CP14.Actions.Components;
using Content.Shared.Actions.Events;
using Content.Shared.Movement.Systems;

namespace Content.Shared._CP14.Actions;

public abstract partial class CP14SharedActionSystem
{
    private void InitializeDoAfter()
    {
        SubscribeLocalEvent<CP14ActionDoAfterSlowdownComponent, CP14ActionStartDoAfterEvent>(OnStartDoAfter);
        SubscribeLocalEvent<CP14ActionDoAfterSlowdownComponent, ActionDoAfterEvent>(OnEndDoAfter);
        SubscribeLocalEvent<CP14SlowdownFromActionsComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMovespeed);
    }

    private void OnStartDoAfter(Entity<CP14ActionDoAfterSlowdownComponent> ent, ref CP14ActionStartDoAfterEvent args)
    {
        var performer = GetEntity(args.Performer);
        EnsureComp<CP14SlowdownFromActionsComponent>(performer, out var slowdown);

        slowdown.SpeedAffectors.Add(ent.Comp.SpeedMultiplier);
        Dirty(performer, slowdown);
        _movement.RefreshMovementSpeedModifiers(performer);
    }

    private void OnEndDoAfter(Entity<CP14ActionDoAfterSlowdownComponent> ent, ref ActionDoAfterEvent args)
    {
        var performer = GetEntity(args.Performer);
        if (!TryComp<CP14SlowdownFromActionsComponent>(performer, out var slowdown))
            return;

        slowdown.SpeedAffectors.Remove(ent.Comp.SpeedMultiplier);
        Dirty(performer, slowdown);

        _movement.RefreshMovementSpeedModifiers(performer);

        if (slowdown.SpeedAffectors.Count == 0)
            RemCompDeferred<CP14SlowdownFromActionsComponent>(performer);
    }

    private void OnRefreshMovespeed(Entity<CP14SlowdownFromActionsComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        var targetSpeedModifier = 1f;

        foreach (var affector in ent.Comp.SpeedAffectors)
        {
            targetSpeedModifier *= affector;
        }

        args.ModifySpeed(targetSpeedModifier);
    }
}
