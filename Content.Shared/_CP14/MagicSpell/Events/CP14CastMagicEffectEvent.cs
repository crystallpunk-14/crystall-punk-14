using Content.Shared._CP14.MagicRitual.Prototypes;
using Content.Shared._CP14.MagicSpell.Components;
using Content.Shared.FixedPoint;
using Content.Shared.Inventory;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.MagicSpell.Events;

/// <summary>
/// Called first to verify that all conditions are met and the spell can be performed.
/// </summary>
[ByRefEvent]
public sealed class CP14CastMagicEffectAttemptEvent : CancellableEntityEventArgs
{
    /// <summary>
    /// The Performer of the event, to check if they meet the requirements.
    /// </summary>
    public EntityUid Performer { get; init; }

    public string Reason = string.Empty;

    public CP14CastMagicEffectAttemptEvent(EntityUid performer)
    {
        Performer = performer;
    }

    public void PushReason(string reason)
    {
        Reason += $"{reason}\n";
    }
}

/// <summary>
/// An event that checks all sorts of conditions, and calculates the total cost of casting a spell. Called before the spell is cast.
/// </summary>
/// <remarks>TODO: This call is duplicated at the beginning of the cast for checks, and at the end of the cast for mana subtraction.</remarks>
public sealed class CP14CalculateManacostEvent : EntityEventArgs, IInventoryRelayEvent
{
    public FixedPoint2 Manacost = 0f;

    public float Multiplier = 1f;
    public EntityUid? Performer;
    public ProtoId<CP14MagicTypePrototype>? MagicType;

    public CP14CalculateManacostEvent(EntityUid? performer, FixedPoint2 initialManacost, ProtoId<CP14MagicTypePrototype>? magicType)
    {
        Performer = performer;
        Manacost = initialManacost;
        MagicType = magicType;
    }

    public float GetManacost()
    {
        return (float)Manacost * Multiplier;
    }

    public SlotFlags TargetSlots { get; } = SlotFlags.All;
}

/// <summary>
/// is invoked if all conditions are met and the spell has begun to be cast (doAfter start moment)
/// </summary>
[ByRefEvent]
public sealed class CP14StartCastMagicEffectEvent : EntityEventArgs
{
    public EntityUid Performer { get; init; }

    public CP14StartCastMagicEffectEvent(EntityUid performer)
    {
        Performer = performer;
    }
}

/// <summary>
/// is invoked on the spell itself when the spell process has been completed or interrupted (doAfter end moment)
/// </summary>
[ByRefEvent]
public sealed class CP14EndCastMagicEffectEvent : EntityEventArgs
{
    public EntityUid Performer { get; init; }

    public CP14EndCastMagicEffectEvent(EntityUid performer)
    {
        Performer = performer;
    }
}

/// <summary>
/// is invoked only if the spell has been successfully cast
/// </summary>
[ByRefEvent]
public sealed class CP14MagicEffectConsumeResourceEvent : EntityEventArgs
{
    public EntityUid? Performer { get; init; }

    public CP14MagicEffectConsumeResourceEvent(EntityUid? performer)
    {
        Performer = performer;
    }
}

[ByRefEvent]
public sealed class CP14SpellFromSpellStorageUsedEvent : EntityEventArgs
{
    public EntityUid? Performer { get; init; }
    public Entity<CP14MagicEffectComponent> Action { get; init; }
    public FixedPoint2 Manacost { get; init; }

    public CP14SpellFromSpellStorageUsedEvent(EntityUid? performer, Entity<CP14MagicEffectComponent> action, FixedPoint2 manacost)
    {
        Performer = performer;
        Action = action;
        Manacost = manacost;
    }
}
