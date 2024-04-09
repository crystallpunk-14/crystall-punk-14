using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Dungeon;

[Prototype("dungeonModifier")]
public sealed partial class CPDungeonLayerModifierPrototype : IPrototype
{
    [IdDataField] public string ID { get; } = default!;

    [DataField]
    public ComponentRegistry Components = default!;
}
