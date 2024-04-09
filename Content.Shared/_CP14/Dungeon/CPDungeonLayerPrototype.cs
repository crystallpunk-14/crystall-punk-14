
using Content.Shared.Parallax.Biomes;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Dungeon;

[Prototype("dungeonLayer")]
public sealed partial class CPDungeonLayerPrototype : IPrototype
{
    [IdDataField] public string ID { get; } = default!;

    [DataField]
    public ProtoId<BiomeTemplatePrototype> BiomeTemplate;

    /// <summary>
    /// minimum and maximum depth for this layer
    /// </summary>
    [DataField]
    public Vector2i Depths = new Vector2i(0, 0);

    [DataField]
    public List<ProtoId<CPDungeonLayerModifierPrototype>> AllowedModifiers = new();
}
