using Content.Shared.Alert;
using Content.Shared.Whitelist;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared._CP14.StatusEffect;

public sealed partial class CP14SharedStatusEffectSystem : EntitySystem
{
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IComponentFactory _compFactory = default!;
    [Dependency] private readonly INetManager _net = default!;

    private EntityQuery<CP14StatusEffectContainerComponent> _containerQuery;
    private EntityQuery<CP14StatusEffectComponent> _effectQuery;

    public override void Initialize()
    {
        base.Initialize();

        InitializeEffects();

        SubscribeLocalEvent<CP14StatusEffectContainerComponent, ComponentShutdown>(OnContainerShutdown);

        SubscribeLocalEvent<CP14StatusEffectComponent, CP14StatusEffectApplied>(OnStatusEffectApplied);
        SubscribeLocalEvent<CP14StatusEffectComponent, CP14StatusEffectRemoved>(OnStatusEffectRemoved);

        _containerQuery = GetEntityQuery<CP14StatusEffectContainerComponent>();
        _effectQuery = GetEntityQuery<CP14StatusEffectComponent>();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        UpdateEffects(frameTime);

        var query = EntityQueryEnumerator<CP14StatusEffectComponent>();
        while (query.MoveNext(out var ent, out var effect))
        {
            if (effect.EndEffectTime is null)
                continue;

            if (!(_timing.CurTime >= effect.EndEffectTime))
                continue;

            if (effect.AppliedTo is null)
                continue;

            var meta = MetaData(ent);

            if (meta.EntityPrototype is null)
                continue;

            TryRemoveStatusEffect(effect.AppliedTo.Value, meta.EntityPrototype);
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

    private void SetStatusEffectTime(EntityUid ent, EntityUid effect, TimeSpan duration)
    {
        if (!_effectQuery.TryComp(effect, out var effectComp))
            return;

        if (effectComp.AppliedTo is null)
            return;

        if (effectComp.Alert is not null)
        {
            effectComp.EndEffectTime = _timing.CurTime + duration;
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
[ByRefEvent]
public readonly record struct CP14StatusEffectApplied(EntityUid Target, Entity<CP14StatusEffectComponent> Effect);

/// <summary>
/// Calls on both effect entity and target entity, when a status effect is removed.
/// </summary>
[ByRefEvent]
public readonly record struct CP14StatusEffectRemoved(EntityUid Target, Entity<CP14StatusEffectComponent> Effect);
