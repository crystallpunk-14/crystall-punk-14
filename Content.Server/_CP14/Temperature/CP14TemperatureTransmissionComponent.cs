namespace Content.Server._CP14.Temperature;

[RegisterComponent, Access(typeof(CP14TemperatureSystem))]
public sealed partial class CP14TemperatureTransmissionComponent : Component
{
    [DataField("containerId", required: true)]
    public string ContainerId = string.Empty;
}

