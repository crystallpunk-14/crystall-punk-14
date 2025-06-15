using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared._CP14.MagicSpell.Events;

public interface ICP14DelayedMagicEffect
{
    public float Cooldown { get; }

    public float CastDelay { get; }

    public bool BreakOnMove { get; }

    public bool BreakOnDamage { get; }

    public float DistanceThreshold { get; }

    public bool Hidden { get; }

    public bool RequireCanInteract { get; }
}

public sealed partial class CP14DelayedWorldTargetActionEvent : WorldTargetActionEvent,
    ICP14DelayedMagicEffect
{
    [DataField]
    public float Cooldown { get; private set; } = 1f;

    [DataField]
    public float CastDelay { get; private set; } = 1f;

    [DataField]
    public bool BreakOnMove { get; private set; } = true;

    [DataField]
    public bool BreakOnDamage { get; private set; } = true;

    [DataField]
    public float DistanceThreshold { get; private set; } = 100f;

    [DataField]
    public bool Hidden { get; private set; } = false;

    [DataField]
    public bool RequireCanInteract { get; private set; } = true;
}

//Entity Target
public sealed partial class CP14DelayedEntityTargetActionEvent : EntityTargetActionEvent,
    ICP14DelayedMagicEffect
{
    [DataField]
    public float Cooldown { get; private set; } = 1f;

    [DataField]
    public float CastDelay { get; private set; } = 1f;

    [DataField]
    public bool BreakOnMove { get; private set; } = true;

    [DataField]
    public bool BreakOnDamage { get; private set; } = true;

    [DataField]
    public float DistanceThreshold { get; private set; } = 100f;

    [DataField]
    public bool Hidden { get; private set; } = false;

    [DataField]
    public bool RequireCanInteract { get; private set; } = true;
}

public sealed partial class CP14DelayedInstantActionEvent : InstantActionEvent, ICP14DelayedMagicEffect
{
    [DataField]
    public float Cooldown { get; private set; } = 3f;

    [DataField]
    public float CastDelay { get; private set; } = 1f;

    [DataField]
    public bool BreakOnMove { get; private set; } = true;

    [DataField]
    public bool BreakOnDamage { get; private set; } = true;

    [DataField]
    public float DistanceThreshold { get; private set; } = 100f;

    [DataField]
    public bool Hidden { get; private set; } = false;

    [DataField]
    public bool RequireCanInteract { get; private set; } = true;
}

[Serializable, NetSerializable]
public sealed partial class CP14DelayedEntityWorldTargetActionDoAfterEvent : DoAfterEvent
{
    [DataField]
    public NetCoordinates? TargetPosition;
    [DataField]
    public NetEntity? TargetEntity;
    [DataField]
    public float? Cooldown;

    public CP14DelayedEntityWorldTargetActionDoAfterEvent(NetCoordinates? targetPos, NetEntity? targetEntity, float cooldown)
    {
        TargetPosition = targetPos;
        TargetEntity = targetEntity;
        Cooldown = cooldown;
    }

    public override DoAfterEvent Clone() => this;
}

[Serializable, NetSerializable]
public sealed partial class CP14DelayedEntityTargetActionDoAfterEvent : DoAfterEvent
{
    [DataField]
    public NetEntity? TargetEntity;
    [DataField]
    public float? Cooldown;

    public CP14DelayedEntityTargetActionDoAfterEvent(NetEntity? targetEntity, float cooldown)
    {
        TargetEntity = targetEntity;
        Cooldown = cooldown;
    }

    public override DoAfterEvent Clone() => this;
}

[Serializable, NetSerializable]
public sealed partial class CP14DelayedInstantActionDoAfterEvent : DoAfterEvent
{
    [DataField]
    public float? Cooldown;

    public CP14DelayedInstantActionDoAfterEvent(float cooldown)
    {
        Cooldown = cooldown;
    }

    public override DoAfterEvent Clone() => this;
}
