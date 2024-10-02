using Content.Shared.DoAfter;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._CP14.MagicRitual;

/// <summary>
/// Called out a ritual when any of its phase triggers are activated
/// </summary>
public sealed class CP14RitualTriggerEvent : EntityEventArgs
{
    public EntProtoId NextPhase;

    public CP14RitualTriggerEvent(EntProtoId phase)
    {
        NextPhase = phase;
    }
}

/// <summary>
/// Called out at a ritual when his stability is altered
/// </summary>
public sealed class CP14RitualStabilityChangedEvent : EntityEventArgs
{
    public float OldStability;
    public float NewStability;

    public CP14RitualStabilityChangedEvent(float oldS, float newS)
    {
        OldStability = oldS;
        NewStability = newS;
    }
}

/// <summary>
/// Called on both the ritual and the phase when they link together
/// </summary>
public sealed class CP14RitualPhaseBoundEvent : EntityEventArgs
{
    public EntityUid Ritual;
    public EntityUid Phase;

    public CP14RitualPhaseBoundEvent(EntityUid r, EntityUid p)
    {
        Ritual = r;
        Phase = p;
    }
}

/// <summary>
/// Invoked at the ritual holder entity when the ritual is complete and the phase entities have been removed
/// </summary>
public sealed class CP14RitualEndEvent : EntityEventArgs
{
    public EntityUid Ritual;

    public CP14RitualEndEvent(EntityUid r)
    {
        Ritual = r;
    }
}

/// <summary>
/// Invoked at the ritual holder entity when the ritual begins, and invokes the starting phase
/// </summary>
public sealed class CP14RitualStartEvent : EntityEventArgs
{
    public EntityUid Ritual;

    public CP14RitualStartEvent(EntityUid r)
    {
        Ritual = r;
    }
}

[Serializable, NetSerializable]
public sealed partial class CP14ActivateRitualDoAfter : SimpleDoAfterEvent
{
}

[Serializable, NetSerializable]
public enum RitualVisuals
{
    Color,
    Enabled,
}
