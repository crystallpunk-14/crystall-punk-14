using Content.Shared.Destructible.Thresholds;
using Content.Shared.Weather;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.WeatherControl;

/// <summary>
/// is the controller that hangs on the prototype map. It regulates which weather rules are run and where they are run.
/// </summary>
[RegisterComponent, AutoGenerateComponentPause, Access(typeof(CP14WeatherControllerSystem))]
public sealed partial class CP14WeatherControllerComponent : Component
{
    [DataField(required: true)]
    public MinMax Delays = default!;

    [DataField, AutoPausedField]
    public TimeSpan NextEventTime = TimeSpan.Zero;

    [DataField]
    public HashSet<ProtoId<WeatherPrototype>> Protos = new();

    [DataField]
    public EntityUid? CurrentEvent;
}
