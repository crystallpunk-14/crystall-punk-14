using Content.Shared.Maps;
using Content.Shared.Tag;
using Robust.Shared.Prototypes;

namespace Content.Shared.Procedural.DungeonLayers;

/// <summary>
/// Generates random placed rooms across the grid
/// </summary>
[Virtual]
public partial class CP14RoomsDunGen : IDunGenLayer
{
    /// <summary>
    /// This rooms can only be generated on the specified tiles
    /// </summary>
    [DataField]
    public HashSet<ProtoId<ContentTileDefinition>>? TileMask;

    /// <summary>
    /// Maximum amount of rooms spawns
    /// </summary>
    [DataField]
    public int Count = 10;

    [DataField(required: true)]
    public HashSet<ProtoId<TagPrototype>> Tags = default!;
}
