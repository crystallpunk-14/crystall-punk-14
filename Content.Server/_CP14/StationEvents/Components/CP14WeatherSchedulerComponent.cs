using Content.Shared.Destructible.Thresholds;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.StationEvents.Components;

/// <summary>
///
/// </summary>
[RegisterComponent, AutoGenerateComponentPause, Access(typeof(CP14WeatherSchedulerSystem))]
public sealed partial class CP14WeatherSchedulerComponent : Component
{
    [DataField(required: true)]
    public MinMax Delays = default!;

    [DataField, AutoPausedField]
    public TimeSpan NextEventTime = TimeSpan.Zero;

    [DataField]
    public HashSet<EntProtoId> Protos = new();

    [DataField]
    public EntityUid? CurrentEvent;
}
