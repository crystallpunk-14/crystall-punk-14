using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Content.Shared.EntityEffects;
using Robust.Shared.Serialization;

namespace Content.Shared._CP14.Magic.Events.Actions;

public sealed partial class CP14DelayedEntityEffectActionEvent : EntityTargetActionEvent, ICP14DelayedMagicEffect
{
    [DataField]
    public float Delay { get; private set; } = 1f;

    [DataField]
    public bool BreakOnMove { get; private set; } = true;

    [DataField]
    public bool BreakOnDamage { get; private set; } = true;
}

[Serializable, NetSerializable]
public sealed partial class CP14CastMagicEffectDoAfterEvent : SimpleDoAfterEvent
{
}
