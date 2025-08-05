using Content.Shared.Maps;
using Content.Shared.Parallax.Biomes;
using Content.Shared.Tag;
using Robust.Shared.Prototypes;

namespace Content.Shared.Procedural.DungeonLayers;

[Virtual]
public partial class CP14BiomeDunGen : IDunGenLayer
{
    [DataField(required: true)]
    public ProtoId<BiomeTemplatePrototype> BiomeTemplate;

    /// <summary>
    /// creates a biome only on the specified tiles
    /// </summary>
    [DataField]
    public HashSet<ProtoId<ContentTileDefinition>>? TileMask;
}
