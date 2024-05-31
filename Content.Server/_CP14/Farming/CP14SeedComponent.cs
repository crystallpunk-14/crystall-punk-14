
namespace Content.Server._CP14.Farming;

[RegisterComponent, Access(typeof(CP14FarmingSystem))]
public sealed partial class CP14SeedComponent : Component
{
    [DataField]
    public float PlantingTime = 2f;
}
