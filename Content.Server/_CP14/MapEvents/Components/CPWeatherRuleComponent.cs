using Content.Shared.Weather;
using Robust.Shared.Prototypes;

namespace Content.Server.CrystallPunk.MapEvents;

/// <summary>
/// 
/// </summary>

[RegisterComponent, Access(typeof(CPWeatherRule))]
public sealed partial class CPWeatherRuleComponent : Component
{
    [DataField(required: true)]
    public ProtoId<WeatherPrototype> Weather = "Storm";

    [DataField]
    public float MinimumLength = 60f;

    [DataField]
    public float MaximumLength = 300f;
}
