namespace Content.Shared._CP14.Farming.Components;

/// <summary>
/// allows the plant to receive energy passively, depending on daylight
/// </summary>
[RegisterComponent, Access(typeof(CP14SharedFarmingSystem))]
public sealed partial class CP14PlantEnergyFromLightComponent : Component
{
    [DataField]
    public float Energy = 0f;

    [DataField]
    public bool Daytime = true;

    [DataField]
    public bool Nighttime = false;
}
