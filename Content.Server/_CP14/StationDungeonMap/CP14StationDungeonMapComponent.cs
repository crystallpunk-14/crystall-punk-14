using Content.Shared.Parallax.Biomes;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.StationDungeonMap;

/// <summary>
/// Initializes a procedurally generated world with points of interest
/// </summary>
[RegisterComponent, Access(typeof(CP14StationDungeonMapSystem))]
public sealed partial class CP14StationDungeonMapComponent : Component
{
    [DataField(required: true)]
    public ProtoId<BiomeTemplatePrototype> Biome = "Caves";

    // If null, its random
    [DataField]
    public int? Seed = null;

    [DataField]
    public Color MapLightColor = Color.Black;

    [DataField]
    public string MapName = "Dungeon map";

    [DataField(serverOnly: true)]
    public ComponentRegistry Components = new();
}
