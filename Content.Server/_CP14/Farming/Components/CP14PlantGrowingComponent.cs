namespace Content.Server._CP14.Farming.Components;

/// <summary>
///
/// </summary>
[RegisterComponent, Access(typeof(CP14FarmingSystem))]
public sealed partial class CP14PlantGrowingComponent : Component
{
    [DataField]
    public float EnergyCost = 1f;

    [DataField]
    public float ResourceCost = 1f;

    [DataField]
    public float GrowthPerUpdate = 0.1f;
}
