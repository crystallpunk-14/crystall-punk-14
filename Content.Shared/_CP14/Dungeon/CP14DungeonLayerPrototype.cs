
using Content.Shared.Parallax.Biomes;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Dungeon;

[Prototype("dungeonLayer")]
public sealed partial class CP14DungeonLayerPrototype : IPrototype
{
    [IdDataField] public string ID { get; } = default!;

    [DataField]
    public ProtoId<BiomeTemplatePrototype> BiomeTemplate;

    /// <summary>
    /// minimum and maximum depth for this layer
    /// </summary>
    [DataField]
    public Vector2i Depths = new(0, 0);

    [DataField]
    public List<ProtoId<CPDungeonLayerModifierPrototype>> Modifiers = new();

    [DataField]
    public List<IDungeonLevel> Levels = new();
}
