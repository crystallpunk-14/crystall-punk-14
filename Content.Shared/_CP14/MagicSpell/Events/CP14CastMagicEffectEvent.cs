using Content.Shared.FixedPoint;
using Content.Shared.Inventory;

namespace Content.Shared._CP14.MagicSpell.Events;

/// <summary>
/// An event that checks all sorts of conditions, and calculates the total cost of casting a spell. Called before the spell is cast.
/// </summary>
/// <remarks>TODO: This call is duplicated at the beginning of the cast for checks, and at the end of the cast for mana subtraction.</remarks>
public sealed class CP14CalculateManacostEvent : EntityEventArgs, IInventoryRelayEvent
{
    public FixedPoint2 Manacost = 0f;

    public float Multiplier = 1f;
    public EntityUid? Performer;

    public CP14CalculateManacostEvent(EntityUid? performer, FixedPoint2 initialManacost)
    {
        Performer = performer;
        Manacost = initialManacost;
    }

    public float GetManacost()
    {
        return (float)Manacost * Multiplier;
    }

    public SlotFlags TargetSlots { get; } = SlotFlags.All;
}

[ByRefEvent]
public sealed class CP14SpellFromSpellStorageUsedEvent(
    EntityUid? performer,
    EntityUid? action,
    FixedPoint2 manacost)
    : EntityEventArgs
{
    public EntityUid? Performer { get; init; } = performer;
    public EntityUid? Action { get; init; } = action;
    public FixedPoint2 Manacost { get; init; } = manacost;
}
