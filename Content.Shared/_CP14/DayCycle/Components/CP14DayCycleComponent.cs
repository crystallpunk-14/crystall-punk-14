using Content.Shared._CP14.DayCycle.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._CP14.DayCycle.Components;

/// <summary>
/// Stores all the necessary data for the day and night cycle system to work.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause, Access(typeof(CP14SharedDayCycleSystem))]
public sealed partial class CP14DayCycleComponent : Component
{
    [ViewVariables]
    public int NextTimeEntryIndex
    {
        get
        {
            if (IndexedCycle is null)
                return 0;
            return CurrentTimeEntryIndex + 1 >= IndexedCycle.TimeEntries.Count ? 0 : CurrentTimeEntryIndex + 1;
        }
    }

    [ViewVariables]
    public DayCycleEntry? CurrentTimeEntry => IndexedCycle?.TimeEntries[CurrentTimeEntryIndex];

    [ViewVariables]
    public DayCycleEntry? NextCurrentTimeEntry => IndexedCycle?.TimeEntries[NextTimeEntryIndex];

    [ViewVariables]
    public Color? StartColor => CurrentTimeEntry?.Color;

    [ViewVariables]
    public Color? EndColor => NextCurrentTimeEntry?.Color;

    [ViewVariables]
    public ProtoId<CP14DayCyclePeriodPrototype>? CurrentPeriod => CurrentTimeEntry?.Period;

    public CP14DayCyclePrototype? IndexedCycle;

    [DataField, AutoNetworkedField]
    public ProtoId<CP14DayCyclePrototype> CycleProto = "Default";

    [DataField, ViewVariables, AutoNetworkedField]
    public int CurrentTimeEntryIndex;

    [DataField, ViewVariables, AutoNetworkedField, AutoPausedField]
    public TimeSpan EntryStartTime;

    [DataField, ViewVariables, AutoNetworkedField, AutoPausedField]
    public TimeSpan EntryEndTime;

    [DataField]
    public bool StartWithRandomEntry = true;
}

[DataDefinition, NetSerializable, Serializable]
public readonly partial record struct DayCycleEntry()
{
    public DayCycleEntry(Color _color, TimeSpan _duration, string _period) : this()
    {
        Color = _color;
        Duration = _duration;
        Period = _period;
    }
    /// <summary>
    /// The color of the world's lights at the beginning of this time of day
    /// </summary>
    [DataField]
    public Color Color { get; init; } = Color.White;

    /// <summary>
    /// Duration of color shift to the next time of day
    /// </summary>
    [DataField]
    public TimeSpan Duration { get; init; } = TimeSpan.FromSeconds(60);

    [DataField]
    public ProtoId<CP14DayCyclePeriodPrototype> Period { get; init; } = "Day";
}

/// <summary>
/// Event raised on map entity, wen day cycle changed.
/// </summary>
[ByRefEvent]
public readonly record struct DayCycleChangedEvent(DayCycleEntry? Entry);
