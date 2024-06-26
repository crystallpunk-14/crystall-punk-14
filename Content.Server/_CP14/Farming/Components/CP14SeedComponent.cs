using Robust.Shared.Prototypes;

namespace Content.Server._CP14.Farming.Components;

/// <summary>
/// a component that allows for the creation of the plant entity on the soil
/// </summary>
[RegisterComponent, Access(typeof(CP14FarmingSystem))]
public sealed partial class CP14SeedComponent : Component
{
    [DataField]
    public TimeSpan PlantingTime = TimeSpan.FromSeconds(2f);

    [DataField(required: true)]
    public EntProtoId PlantProto;
}
