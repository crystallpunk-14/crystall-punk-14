using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.StatusEffect;

public sealed partial class CP14SharedStatusEffectSystem
{
    public bool TryAddStatusEffect(EntityUid uid, EntProtoId effectProto, TimeSpan? duration = null)
    {
        EntityUid? effect = null;
        if (TryGetStatusEffect(uid, effectProto, out effect))
        {
            //We dont need to add the effect if it already exists
            if (duration is null)
                return true;

            if (effect != null)
            {
                AddStatusEffectTime(uid, effect.Value, duration.Value);
                return true;
            }
        }

        EnsureComp<CP14StatusEffectContainerComponent>(uid, out var container);

        effect = Spawn(effectProto);

        if (!_effectQuery.TryComp(effect, out var effectComp))
        {
            Log.Error(
                $"Entity {effect} does not have a {nameof(CP14StatusEffectComponent)} component, but tried to apply it as a status effect on {ToPrettyString(uid):player}.");
            QueueDel(effect);
            return false;
        }

        // Duration configure
        if (duration != null)
        {
            effectComp.EndEffectTime = _timing.CurTime + duration;
        }

        container.ActiveStatusEffects.Add(effectProto, effect.Value);
        effectComp.AppliedTo = uid;

        var ev = new CP14StatusEffectApplied(uid, effect.Value);
        RaiseLocalEvent(uid, ev);
        RaiseLocalEvent(effect.Value, ev);

        return true;
    }

    public bool TryRemoveStatusEffect(EntityUid uid, EntProtoId effectProto)
    {
        if (!_containerQuery.TryComp(uid, out var container))
            return false;

        if (!container.ActiveStatusEffects.TryGetValue(effectProto, out var statusEffect))
            return false;

        var ev = new CP14StatusEffectRemoved(uid, statusEffect);
        RaiseLocalEvent(uid, ev);
        RaiseLocalEvent(statusEffect, ev);

        QueueDel(statusEffect);
        container.ActiveStatusEffects.Remove(effectProto);
        return true;
    }

    public bool HasStatusEffect(EntityUid uid, EntProtoId effectProto)
    {
        if (!_containerQuery.TryComp(uid, out var container))
            return false;

        return container.ActiveStatusEffects.ContainsKey(effectProto);
    }

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
