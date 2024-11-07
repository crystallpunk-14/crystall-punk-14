using Content.Shared._CP14.MagicSpell.Components;
using Content.Shared._CP14.MagicSpell.Events;
using Content.Shared.Movement.Systems;

namespace Content.Shared._CP14.MagicSpell;

public abstract partial class CP14SharedMagicSystem
{
    private void InitializeSlowdown()
    {
        SubscribeLocalEvent<CP14MagicEffectCastSlowdownComponent, CP14StartCastMagicEffectEvent>(OnSlowdownCaster);
        SubscribeLocalEvent<CP14MagicEffectCastSlowdownComponent, CP14EndCastMagicEffectEvent>(OnUnslowdownCaster);
        SubscribeLocalEvent<CP14MagicCasterSlowdownComponent, RefreshMovementSpeedModifiersEvent>(OnCasterRefreshMovespeed);
    }

    private void OnSlowdownCaster(Entity<CP14MagicEffectCastSlowdownComponent> ent, ref CP14StartCastMagicEffectEvent args)
    {
        if (!TryComp<CP14MagicCasterSlowdownComponent>(args.Performer, out var caster))
            return;

        caster.SpeedModifier = ent.Comp.SpeedMultiplier;
        _movement.RefreshMovementSpeedModifiers(args.Performer);
    }

    private void OnUnslowdownCaster(Entity<CP14MagicEffectCastSlowdownComponent> ent, ref CP14EndCastMagicEffectEvent args)
    {
        if (!TryComp<CP14MagicCasterSlowdownComponent>(args.Performer, out var caster))
            return;

        caster.SpeedModifier = 1f;

        _movement.RefreshMovementSpeedModifiers(args.Performer);
    }

    private void OnCasterRefreshMovespeed(Entity<CP14MagicCasterSlowdownComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        args.ModifySpeed(ent.Comp.SpeedModifier);
    }
}
