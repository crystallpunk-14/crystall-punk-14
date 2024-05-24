
using Robust.Shared.Serialization;

namespace Content.Shared._CP14.DayCycle;

/// <summary>
/// Stores all the necessary data for the day and night cycle system to work
/// </summary>

[RegisterComponent, Access(typeof(DayCycleSystem))]
public sealed partial class DayCycleComponent : Component
{
    public int NextTimeEntry => CurrentTimeEntry + 1 >= TimeEntries.Count ? 0 : CurrentTimeEntry + 1;
    public Color StartColor => TimeEntries[CurrentTimeEntry].StartColor;
    public Color EndColor => TimeEntries[NextTimeEntry].StartColor;

    [DataField(required: true)]
    public List<DayCycleEntry> TimeEntries = new();

    [DataField]
    public bool IsNight; // TODO: Rewrite this shit

    [DataField]
    public int CurrentTimeEntry;

    [DataField]
    public TimeSpan EntryStartTime;

    [DataField]
    public TimeSpan EntryEndTime;
}

[DataDefinition, NetSerializable, Serializable]
public readonly partial record struct DayCycleEntry()
{
    /// <summary>
    /// The color of the world's lights at the beginning of this time of day
    /// </summary>
    [DataField]
    public Color StartColor { get; init; } = Color.White; // TODO: Rename it to "Color"?

    /// <summary>
    /// Duration of color shift to the next time of day
    /// </summary>
    [DataField]
    public TimeSpan Duration { get; init; } = TimeSpan.FromSeconds(60);

    [DataField]
    public bool IsNight { get; init; } = false;
}

/// <summary>
/// Event raised on map entity, wen night is started
/// </summary>
[ByRefEvent]
public readonly record struct DayCycleNightStartedEvent(EntityUid Map);

/// <summary>
/// Event raised on map entity, wen night is started
/// </summary>
[ByRefEvent]
public readonly record struct DayCycleDayStartedEvent(EntityUid Map);
