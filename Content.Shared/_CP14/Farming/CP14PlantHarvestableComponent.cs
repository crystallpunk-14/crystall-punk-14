
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Farming;

/// <summary>
/// Means that the plant can be harvested.
/// </summary>
[RegisterComponent]
public sealed partial class CP14PlantHarvestableComponent : Component
{
    [DataField]
    public List<EntProtoId> Harvest = new();

    [DataField]
    public TimeSpan Time = TimeSpan.FromSeconds(1f);

    /// <summary>
    /// after harvesting, should the plant be completely removed?
    /// </summary>
    [DataField]
    public bool DeleteAfterHarvest = false;

    /// <summary>
    /// after harvest, the growth level of the plant will be reduced by the specified value
    /// </summary>
    [DataField]
    public float GrowthCostHarvest = 0.4f;

    /// <summary>
    /// what level of growth does a plant need to have before it can be harvested?
    /// </summary>
    [DataField]
    public float GrowthLevelToHarvest = 0.9f;
}
