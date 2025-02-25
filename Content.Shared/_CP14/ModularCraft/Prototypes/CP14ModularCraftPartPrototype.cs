using Content.Shared.Hands.Components;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.ModularCraft.Prototypes;

[Prototype("modularPart")]
public sealed partial class CP14ModularCraftPartPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    public List<ProtoId<CP14ModularCraftSlotPrototype>> Slots = new();

    /// <summary>
    /// An entity that can drop out of the final modular item when destroyed.
    /// By design, the original item with this prototype from which the weapon was assembled.
    /// </summary>
    [DataField]
    public EntProtoId? SourcePart;

    [DataField]
    public float DestroyProb;

    [DataField(serverOnly: true)]
    public List<CP14ModularCraftModifier> Modifiers = new();

    [DataField]
    public string? RsiPath;

    /// <summary>
    /// Automatic colored all states, QoL for YML size reducing
    /// </summary>
    [DataField]
    public Color? Color;

    [DataField]
    public List<PrototypeLayerData>? IconSprite;

    [DataField]
    public Dictionary<HandLocation, List<PrototypeLayerData>>? InhandVisuals;

    [DataField]
    public Dictionary<HandLocation, List<PrototypeLayerData>>? WieldedInhandVisuals;

    [DataField]
    public Dictionary<string, List<PrototypeLayerData>>? ClothingVisuals;
}
