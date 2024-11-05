using Content.Shared.FixedPoint;

namespace Content.Shared._CP14.MagicSpell.Events;

[ByRefEvent]
public sealed class CP14BeforeCastMagicEffectEvent : CancellableEntityEventArgs
{
    /// <summary>
    /// The Performer of the event, to check if they meet the requirements.
    /// </summary>
    public EntityUid Caster { get; init; }

    public string Reason = string.Empty;

    public CP14BeforeCastMagicEffectEvent(EntityUid caster)
    {
        Caster = caster;
    }

    public void PushReason(string reason)
    {
        Reason += $"{reason}\n";
    }
}

[ByRefEvent]
public sealed class CP14AfterCastMagicEffectEvent : EntityEventArgs
{
    public EntityUid? Caster { get; init; }

    public CP14AfterCastMagicEffectEvent(EntityUid caster)
    {
        Caster = caster;
    }
}
/// <summary>
/// is invoked if all conditions are met and the spell has begun to be cast
/// </summary>
[ByRefEvent]
public sealed class CP14StartCastMagicEffectEvent : EntityEventArgs
{
    public EntityUid Caster { get; init; }

    public CP14StartCastMagicEffectEvent(EntityUid caster)
    {
        Caster = caster;
    }
}

/// <summary>
/// is invoked on the spell itself when the spell process has been completed or interrupted
/// </summary>
[ByRefEvent]
public sealed class CP14EndCastMagicEffectEvent : EntityEventArgs
{
    public EntityUid Caster { get; init; }

    public CP14EndCastMagicEffectEvent(EntityUid caster)
    {
        Caster = caster;
    }
}

public sealed class CP14CalculateManacostEvent : EntityEventArgs
{
    public FixedPoint2 Manacost = 0f;

    public float Multiplier = 1f;
    public EntityUid Caster;

    public CP14CalculateManacostEvent(EntityUid caster, FixedPoint2 initialManacost)
    {
        Caster = caster;
        Manacost = initialManacost;
    }

    public float GetManacost()
    {
        return (float)Manacost * Multiplier;
    }
}
