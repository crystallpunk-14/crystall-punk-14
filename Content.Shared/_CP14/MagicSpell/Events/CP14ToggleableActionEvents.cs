using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._CP14.MagicSpell.Events;

public interface ICP14ToggleableMagicEffect
{
    public float EffectFrequency { get; }

    public float CastTime { get; }

    public float Cooldown { get; }

    public bool BreakOnMove { get; }

    public bool BreakOnDamage { get; }

    public bool Hidden{ get; }

    public float EntityDistance { get; }
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
    public bool Hidden { get; private set; } = false;

    [DataField]
    public float EntityDistance { get; private set; } = 100f;
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
