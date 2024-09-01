using Content.Shared.Destructible.Thresholds;
using Content.Shared.Weather;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.WeatherControl;

[DataRecord, Serializable]
public sealed class CP14WeatherData
{
    [DataField(required: true)]
    public ProtoId<WeatherPrototype> Visuals { get; set; }

    [DataField]
    public MinMax Duration { get; set; } = new(30, 300);
}
