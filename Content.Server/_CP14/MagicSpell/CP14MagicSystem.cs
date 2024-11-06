using Content.Server.Chat.Systems;
using Content.Shared._CP14.MagicSpell;
using Content.Shared._CP14.MagicSpell.Components;
using Content.Shared._CP14.MagicSpell.Events;
using Content.Shared.Movement.Systems;
using Robust.Server.GameObjects;

namespace Content.Server._CP14.MagicSpell;

public sealed partial class CP14MagicSystem : CP14SharedMagicSystem
{
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movement = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<CP14MagicEffectVerbalAspectComponent, CP14VerbalAspectSpeechEvent>(OnSpellSpoken);

        SubscribeLocalEvent<CP14MagicEffectCastingVisualComponent, CP14StartCastMagicEffectEvent>(OnSpawnMagicVisualEffect);
        SubscribeLocalEvent<CP14MagicEffectCastingVisualComponent, CP14EndCastMagicEffectEvent>(OnDespawnMagicVisualEffect);

        SubscribeLocalEvent<CP14MagicEffectCastSlowdownComponent, CP14StartCastMagicEffectEvent>(OnSlowdownCaster);
        SubscribeLocalEvent<CP14MagicEffectCastSlowdownComponent, CP14EndCastMagicEffectEvent>(OnUnslowdownCaster);
        SubscribeLocalEvent<CP14MagicCasterSlowdownComponent, RefreshMovementSpeedModifiersEvent>(OnCasterRefreshMovespeed);
    }

    private void OnSlowdownCaster(Entity<CP14MagicEffectCastSlowdownComponent> ent, ref CP14StartCastMagicEffectEvent args)
    {
        if (!TryComp<CP14MagicCasterSlowdownComponent>(args.Caster, out var caster))
            return;

        caster.SpeedModifiers.Add(ent.Comp.SpeedMultiplier);
        _movement.RefreshMovementSpeedModifiers(args.Caster);
    }

    private void OnUnslowdownCaster(Entity<CP14MagicEffectCastSlowdownComponent> ent, ref CP14EndCastMagicEffectEvent args)
    {
        if (!TryComp<CP14MagicCasterSlowdownComponent>(args.Caster, out var caster))
            return;

        if (caster.SpeedModifiers.Contains(ent.Comp.SpeedMultiplier))
            caster.SpeedModifiers.Remove(ent.Comp.SpeedMultiplier);

        _movement.RefreshMovementSpeedModifiers(args.Caster);
    }

    private void OnCasterRefreshMovespeed(Entity<CP14MagicCasterSlowdownComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        var result = 1f;
        foreach (var modifier in ent.Comp.SpeedModifiers)
        {
            result += modifier;
        }

        args.ModifySpeed(result);
    }

    private void OnSpellSpoken(Entity<CP14MagicEffectVerbalAspectComponent> ent, ref CP14VerbalAspectSpeechEvent args)
    {
        if (args.Performer is not null && args.Speech is not null)
            _chat.TrySendInGameICMessage(args.Performer.Value, args.Speech, InGameICChatType.Speak, true);
    }

    private void OnSpawnMagicVisualEffect(Entity<CP14MagicEffectCastingVisualComponent> ent, ref CP14StartCastMagicEffectEvent args)
    {
        var vfx = SpawnAttachedTo(ent.Comp.Proto, Transform(args.Caster).Coordinates);
        _transform.SetParent(vfx, args.Caster);
        ent.Comp.SpawnedEntity = vfx;
    }

    private void OnDespawnMagicVisualEffect(Entity<CP14MagicEffectCastingVisualComponent> ent, ref CP14EndCastMagicEffectEvent args)
    {
        QueueDel(ent.Comp.SpawnedEntity);
        ent.Comp.SpawnedEntity = null;
    }
}
