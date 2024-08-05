using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared._CP14.MagicSpell.Events;

//World target
public sealed partial class CP14DelayedWorldTargetActionEvent : WorldTargetActionEvent, ICP14DelayedMagicEffect
{
    [DataField]
    public float Delay { get; private set; } = 1f;

    [DataField]
    public bool BreakOnMove { get; private set; } = true;

    [DataField]
    public bool BreakOnDamage { get; private set; } = true;

    [DataField]
    public bool Hidden { get; private set; } = false;
}

[Serializable, NetSerializable]
public sealed partial class CP14DelayedWorldTargetActionDoAfterEvent : DoAfterEvent
{
    [DataField]
    public NetCoordinates Target;
    public override DoAfterEvent Clone() => this;
}


//Entity Target
public sealed partial class CP14DelayedEntityTargetActionEvent : EntityTargetActionEvent, ICP14DelayedMagicEffect
{
    [DataField]
    public float Delay { get; private set; } = 1f;

    [DataField]
    public bool BreakOnMove { get; private set; } = true;

    [DataField]
    public bool BreakOnDamage { get; private set; } = true;

    [DataField]
    public bool Hidden { get; private set; } = false;
}

[Serializable, NetSerializable]
public sealed partial class CP14DelayedEntityTargetActionDoAfterEvent : SimpleDoAfterEvent
{
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
}

[Serializable, NetSerializable]
public sealed partial class CP14DelayedInstantActionDoAfterEvent : SimpleDoAfterEvent
{
}
