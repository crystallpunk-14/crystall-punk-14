using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.StatusEffect;

public sealed partial class CP14SharedStatusEffectSystem
{
    /// <summary>
    /// Attempts to add a status effect to the specified entity. Returns True if the effect is added or exists
    /// and has been successfully extended in time, returns False if the status effect cannot be applied to this entity,
    /// or for any other reason
    /// </summary>
    /// <param name="uid">The target entity on which the effect is added</param>
    /// <param name="effectProto">ProtoId of the status effect entity. Make sure it has CP14StatusEffectComponent on it</param>
    /// <param name="duration">Duration of status effect. Leave null and the effect will be permanent until it is removed using <c>TryRemoveStatusEffect</c>
    /// In the other case, the effect will either be added for the specified time or its time will be extended for the specified time.</param>
    /// <returns></returns>
    public bool TryAddStatusEffect(EntityUid uid, EntProtoId effectProto, TimeSpan? duration = null)
    {
        if (TryGetStatusEffect(uid, effectProto, out var existedEffect))
        {
            //We don't need to add the effect if it already exists
            if (duration is null)
                return true;

            if (existedEffect != null)
            {
                AddStatusEffectTime(uid, existedEffect.Value, duration.Value);
                return true;
            }
        }

        // We make this checks before status effect entity spawned
        if (!_proto.TryIndex(effectProto, out var effectProtoData))
            return false;

        if (!effectProtoData.TryGetComponent<CP14StatusEffectComponent>(out var effectProtoComp, _compFactory))
        {
            Log.Error($"Entity {effectProto} does not have a {nameof(CP14StatusEffectComponent)} component, but tried to apply it as a status effect on {ToPrettyString(uid)}.");
            return false;
        }

        if (effectProtoComp.Whitelist is not null && !_whitelist.IsValid(effectProtoComp.Whitelist, uid))
            return false;

        EnsureComp<CP14StatusEffectContainerComponent>(uid, out var container);

        //And only if all checks passed we spawn the effect
        var effect = Spawn(effectProto);

        if (!_effectQuery.TryComp(effect, out var effectComp))
            return false;

        if (duration != null)
            effectComp.EndEffectTime = _timing.CurTime + duration;

        container.ActiveStatusEffects.Add(effectProto, effect);
        effectComp.AppliedTo = uid;

        var ev = new CP14StatusEffectApplied(uid, (effect, effectComp));
        RaiseLocalEvent(uid, ev);
        RaiseLocalEvent(effect, ev);

        return true;
    }

    /// <summary>
    /// Attempting to remove a status effect from an entity. Returns True if the status effect existed on the entity and was successfully removed, and False in any other case.
    /// </summary>
    public bool TryRemoveStatusEffect(EntityUid uid, EntProtoId effectProto)
    {
        if (!_containerQuery.TryComp(uid, out var container))
            return false;

        if (!container.ActiveStatusEffects.TryGetValue(effectProto, out var effect))
            return false;

        if (!_effectQuery.TryComp(effect, out var effectComp))
            return false;

        var ev = new CP14StatusEffectRemoved(uid, (effect, effectComp));
        RaiseLocalEvent(uid, ev);
        RaiseLocalEvent(effect, ev);

        QueueDel(effect);
        container.ActiveStatusEffects.Remove(effectProto);
        return true;
    }

    /// <summary>
    /// Checks whether the specified entity is under a specific status effect.
    /// </summary>
    public bool HasStatusEffect(EntityUid uid, EntProtoId effectProto)
    {
        if (!_containerQuery.TryComp(uid, out var container))
            return false;

        return container.ActiveStatusEffects.ContainsKey(effectProto);
    }

    /// <summary>
    /// Attempting to retrieve the EntityUid of a status effect from an entity.
    /// </summary>
    public bool TryGetStatusEffect(EntityUid uid, EntProtoId effectProto, out EntityUid? effect)
    {
        if (!_containerQuery.TryComp(uid, out var container))
        {
            effect = null;
            return false;
        }

        if (container.ActiveStatusEffects.TryGetValue(effectProto, out var statusEffect))
        {
            effect = statusEffect;
            return true;
        }

        effect = null;
        return false;
    }
}
