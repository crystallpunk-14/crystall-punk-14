namespace Content.Server._CP14.Farming.Components;

/// <summary>
/// allows the plant to receive energy passively, depending on daylight
/// </summary>
[RegisterComponent, Access(typeof(CP14FarmingSystem))]
public sealed partial class CP14PlantEnergyFromLightComponent : Component
{
    [DataField]
    public float Energy = 1f;

    [DataField]
    public bool Daylight = true;

    [DataField]
    public bool Dark = false;

    [DataField]
    public float MinUpdateFrequency = 30f;

    [DataField]
    public float MaxUpdateFrequency = 90f;

    [DataField]
    public TimeSpan NextUpdateTime = TimeSpan.Zero;
}
