using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared._CP14.MagicSpell.Events;

public sealed partial class CP14DelayedEntityWorldTargetActionEvent : EntityWorldTargetActionEvent,
    ICP14DelayedMagicEffect
{
    [DataField]
    public float Delay { get; private set; } = 1f;

    [DataField]
    public bool BreakOnMove { get; private set; } = true;

    [DataField]
    public bool BreakOnDamage { get; private set; } = true;

    [DataField]
    public bool Hidden { get; private set; } = false;

    [DataField]
    public float EntityDistance { get; private set; } = 100f;
}

[Serializable, NetSerializable]
public sealed partial class CP14DelayedEntityWorldTargetActionDoAfterEvent : DoAfterEvent
{
    [DataField]
    public NetCoordinates? TargetPosition;
    [DataField]
    public NetEntity? TargetEntity;

    public CP14DelayedEntityWorldTargetActionDoAfterEvent(NetCoordinates? targetPos, NetEntity? targetEntity)
    {
        TargetPosition = targetPos;
        TargetEntity = targetEntity;
    }

    public override DoAfterEvent Clone() => this;
}

//Instant
public sealed partial class CP14DelayedInstantActionEvent : InstantActionEvent, ICP14DelayedMagicEffect
{
    [DataField]
    public float Delay { get; private set; } = 1f;

    [DataField]
    public bool BreakOnMove { get; private set; } = true;

    [DataField]
    public bool BreakOnDamage { get; private set; } = true;

    [DataField]
    public bool Hidden { get; private set; } = false;

    [DataField]
    public float EntityDistance { get; private set; } = 100f;
}

[Serializable, NetSerializable]
public sealed partial class CP14DelayedInstantActionDoAfterEvent : SimpleDoAfterEvent
{
}
