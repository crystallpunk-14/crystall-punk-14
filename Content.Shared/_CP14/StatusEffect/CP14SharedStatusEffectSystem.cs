using Content.Shared.Alert;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;

namespace Content.Shared._CP14.StatusEffect;

public sealed partial class CP14SharedStatusEffectSystem : EntitySystem
{
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private EntityQuery<CP14StatusEffectContainerComponent> _containerQuery;
    private EntityQuery<CP14StatusEffectComponent> _effectQuery;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14StatusEffectContainerComponent, ComponentShutdown>(OnContainerShutdown);

        SubscribeLocalEvent<CP14StatusEffectComponent, CP14StatusEffectApplied>(OnStatusEffectApplied);
        SubscribeLocalEvent<CP14StatusEffectComponent, CP14StatusEffectRemoved>(OnStatusEffectRemoved);

        _containerQuery = GetEntityQuery<CP14StatusEffectContainerComponent>();
        _effectQuery = GetEntityQuery<CP14StatusEffectComponent>();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<CP14StatusEffectComponent>();
        while (query.MoveNext(out var ent, out var effect))
        {
            if (effect.EndEffectTime is null)
                continue;

            if (!(_timing.CurTime >= effect.EndEffectTime))
                continue;

            if (effect.AppliedTo is null)
                continue;

            var ev = new CP14StatusEffectRemoved(effect.AppliedTo.Value, ent);
            RaiseLocalEvent(effect.AppliedTo.Value, ev);
            RaiseLocalEvent(ent, ev);

            EnsureComp<CP14StatusEffectContainerComponent>(effect.AppliedTo.Value, out var container);

            var meta = MetaData(ent);
            if (meta.EntityPrototype is not null)
                container.ActiveStatusEffects.Remove(meta.EntityPrototype);

            QueueDel(ent);
        }
    }

    private void OnContainerShutdown(Entity<CP14StatusEffectContainerComponent> ent, ref ComponentShutdown args)
    {
        foreach (var effect in ent.Comp.ActiveStatusEffects)
        {
            QueueDel(effect.Value);
        }
    }

    private void AddStatusEffectTime(EntityUid ent, EntityUid effect, TimeSpan duration)
    {
        if (!_effectQuery.TryComp(effect, out var effectComp))
            return;

        if (effectComp.AppliedTo is null)
            return;

        if (effectComp.Alert is not null)
        {
            effectComp.EndEffectTime += duration;
            _alerts.ShowAlert(
                effectComp.AppliedTo.Value,
                effectComp.Alert.Value,
                cooldown: effectComp.EndEffectTime is null ? null : (_timing.CurTime, effectComp.EndEffectTime.Value));
        }
    }

    private void OnStatusEffectApplied(Entity<CP14StatusEffectComponent> ent, ref CP14StatusEffectApplied args)
    {
        if (ent.Comp.AppliedTo is null)
            return;

        if (ent.Comp.Alert is not null)
        {
            _alerts.ShowAlert(
                ent.Comp.AppliedTo.Value,
                ent.Comp.Alert.Value,
                cooldown: ent.Comp.EndEffectTime is null ? null : (_timing.CurTime, ent.Comp.EndEffectTime.Value));
        }
    }

    private void OnStatusEffectRemoved(Entity<CP14StatusEffectComponent> ent, ref CP14StatusEffectRemoved args)
    {
        if (ent.Comp.AppliedTo is null)
            return;

        if (ent.Comp.Alert is not null)
        {
            _alerts.ClearAlert(ent.Comp.AppliedTo.Value, ent.Comp.Alert.Value);
        }
    }
}

/// <summary>
/// Calls on both effect entity and target entity, when a status effect is applied.
/// </summary>
public sealed class CP14StatusEffectApplied(EntityUid target, EntityUid effect) : EntityEventArgs
{
    public EntityUid Target = target;
    public EntityUid Effect = effect;
}

/// <summary>
/// Calls on both effect entity and target entity, when a status effect is removed.
/// </summary>
public sealed class CP14StatusEffectRemoved(EntityUid target, EntityUid effect) : EntityEventArgs
{
    public EntityUid Target = target;
    public EntityUid Effect = effect;
}
