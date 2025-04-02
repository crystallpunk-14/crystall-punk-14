namespace Content.Shared._CP14.Farming.Components;

/// <summary>
/// a component that provides a link to a liquid storage that can be used by the plant
/// </summary>
[RegisterComponent, Access(typeof(CP14SharedFarmingSystem))]
public sealed partial class CP14SoilComponent : Component
{
    [DataField(required: true)]
    public string Solution = string.Empty;

    public EntityUid? PlantUid;
}
