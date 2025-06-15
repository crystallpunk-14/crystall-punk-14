using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared._CP14.MagicSpell.Events;

public interface ICP14ToggleableMagicEffect
{
    public float EffectFrequency { get; }

    public float CastTime { get; }

    public float Cooldown { get; }

    public bool BreakOnMove { get; }

    public bool BreakOnDamage { get; }

    public float DistanceThreshold { get; }

    public bool Hidden{ get; }
}

public sealed partial class CP14ToggleableInstantActionEvent : InstantActionEvent, ICP14ToggleableMagicEffect
{
    [DataField]
    public float EffectFrequency { get; private set; } = 1f;

    [DataField]
    public float Cooldown { get; private set; } = 3f;

    [DataField]
    public float CastTime { get; private set; } = 10f;

    [DataField]
    public bool BreakOnMove { get; private set; } = false;

    [DataField]
    public bool BreakOnDamage { get; private set; } = true;

    [DataField]
    public float DistanceThreshold { get; private set; } = 100f;

    [DataField]
    public bool Hidden { get; private set; } = false;
}

public sealed partial class CP14ToggleableWorldTargetActionEvent : WorldTargetActionEvent, ICP14ToggleableMagicEffect
{
    [DataField]
    public float EffectFrequency { get; private set; } = 1f;

    [DataField]
    public float Cooldown { get; private set; } = 3f;

    [DataField]
    public float CastTime { get; private set; } = 10f;

    [DataField]
    public bool BreakOnMove { get; private set; } = false;

    [DataField]
    public bool BreakOnDamage { get; private set; } = true;

    [DataField]
    public float DistanceThreshold { get; private set; } = 100f;

    [DataField]
    public bool Hidden { get; private set; } = false;
}

public sealed partial class CP14ToggleableEntityTargetActionEvent : EntityTargetActionEvent, ICP14ToggleableMagicEffect
{
    [DataField]
    public float EffectFrequency { get; private set; } = 1f;

    [DataField]
    public float Cooldown { get; private set; } = 3f;

    [DataField]
    public float CastTime { get; private set; } = 10f;

    [DataField]
    public bool BreakOnMove { get; private set; } = false;

    [DataField]
    public bool BreakOnDamage { get; private set; } = true;

    [DataField]
    public float DistanceThreshold { get; private set; } = 100f;

    [DataField]
    public bool Hidden { get; private set; } = false;
}

[Serializable, NetSerializable]
public sealed partial class CP14ToggleableInstantActionDoAfterEvent : DoAfterEvent
{
    [DataField]
    public float? Cooldown;

    public CP14ToggleableInstantActionDoAfterEvent(float cooldown)
    {
        Cooldown = cooldown;
    }

    public override DoAfterEvent Clone() => this;
}

[Serializable, NetSerializable]
public sealed partial class CP14ToggleableEntityWorldTargetActionDoAfterEvent : DoAfterEvent
{
    [DataField]
    public NetCoordinates? TargetPosition;
    [DataField]
    public NetEntity? TargetEntity;
    [DataField]
    public float? Cooldown;

    public CP14ToggleableEntityWorldTargetActionDoAfterEvent(NetCoordinates? targetPos, NetEntity? targetEntity, float cooldown)
    {
        TargetPosition = targetPos;
        TargetEntity = targetEntity;
        Cooldown = cooldown;
    }

    public override DoAfterEvent Clone() => this;
}


[Serializable, NetSerializable]
public sealed partial class CP14ToggleableEntityTargetActionDoAfterEvent : DoAfterEvent
{
    [DataField]
    public NetEntity? TargetEntity;
    [DataField]
    public float? Cooldown;

    public CP14ToggleableEntityTargetActionDoAfterEvent(NetEntity? targetEntity, float cooldown)
    {
        TargetEntity = targetEntity;
        Cooldown = cooldown;
    }

    public override DoAfterEvent Clone() => this;
}
