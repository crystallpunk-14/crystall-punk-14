namespace Content.Server._CP14.Temperature;

[RegisterComponent, Access(typeof(CP14TemperatureTransmissionSystem))]
public sealed partial class CP14TemperatureTransmissionComponent : Component
{
    [DataField("transmissionRate")]
    public float TransmissionRate = 0.1f;

    [DataField("maxTemperature")]
    public float MaxTemperature = 450.0f;

    [DataField("containerId", required: true)]
    public string ContainerId = string.Empty;
}

