using Content.Shared.Hands.Components;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.ModularCraft.Prototypes;

[Prototype("modularPart")]
public sealed partial class CP14ModularCraftPartPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    public ProtoId<CP14ModularCraftSlotPrototype> TargetSlot = string.Empty;

    /// <summary>
    /// An entity that can drop out of the final modular item when destroyed.
    /// By design, the original item with this prototype from which the weapon was assembled.
    /// </summary>
    [DataField]
    public EntProtoId? SourcePart = string.Empty;

    [DataField]
    public float DestroyProb = 0.25f;

    [DataField(serverOnly: true)]
    public List<CP14ModularCraftModifier> Modifiers = new();

    [DataField]
    public HashSet<ProtoId<CP14ModularCraftSlotPrototype>> AddSlots = new();

    [DataField]
    public string? RsiPath;

    [DataField]
    public List<PrototypeLayerData>? IconSprite;

    [DataField]
    public Dictionary<HandLocation, List<PrototypeLayerData>>? InhandVisuals;

    [DataField]
    public Dictionary<HandLocation, List<PrototypeLayerData>>? WieldedInhandVisuals;

    [DataField]
    public Dictionary<string, List<PrototypeLayerData>>? ClothingVisuals;

    //Clothing
}
