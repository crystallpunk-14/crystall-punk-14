using Content.Server._CP14.StationEvents.Events;
using Content.Shared.Weather;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.StationEvents.Components;

[RegisterComponent, Access(typeof(CP14WeatherRule))]
public sealed partial class CP14WeatherRuleComponent : Component
{
    [DataField(required: true)]
    public ProtoId<WeatherPrototype> Weather = default!;

    public MapId? Map = null;
}
