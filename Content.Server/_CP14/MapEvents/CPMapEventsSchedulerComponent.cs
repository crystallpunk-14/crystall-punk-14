using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Server.CrystallPunk.MapEvents;

/// <summary>
/// 
/// </summary>

[RegisterComponent, Access(typeof(CPMapEventsSchedulerSystem))]
public sealed partial class CPMapEventsSchedulerComponent : Component
{
    public MapId Map;

    [DataField]
    public float MinimumTimeUntilFirstEvent = 300f;

    [DataField]
    public float MinimumTimeBetweenEvents = 30f;

    [DataField]
    public float MaximumTimeBetweenEvents = 300f;

    [DataField(required: true)]
    public List<EntProtoId> WhitelistedEvents = new();


    /// <summary>
    /// How long until the next check for an event runs
    /// </summary>
    [DataField]
    public TimeSpan NextEventTime;
}
