using Content.Shared.Maps;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Farming.Components;

/// <summary>
/// a component that allows for the creation of the entity on the tile
/// </summary>
[RegisterComponent, Access(typeof(CP14SharedFarmingSystem))]
public sealed partial class CP14SeedComponent : Component
{
    [DataField]
    public TimeSpan PlantingTime = TimeSpan.FromSeconds(2f);

    [DataField(required: true)]
    public EntProtoId PlantProto;

    [DataField(required: true)]
    public HashSet<ProtoId<ContentTileDefinition>> SoilTile = new();
}
