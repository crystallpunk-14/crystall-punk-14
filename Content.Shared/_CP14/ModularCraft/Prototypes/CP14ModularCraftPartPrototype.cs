using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.ModularCraft.Prototypes;

[Prototype("modularPart")]
public sealed partial class CP14ModularCraftPartPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public ProtoId<CP14ModularCraftSlotPrototype> TargetSlot = string.Empty;

    /// <summary>
    /// An entity that can drop out of the final modular item when destroyed.
    /// By design, the original item with this prototype from which the weapon was assembled.
    /// </summary>
    [DataField]
    public List<EntProtoId> SourceParts = new();

    [DataField]
    public List<CP14ModularCraftModifier> Modifiers = new();

    [DataField]
    public HashSet<ProtoId<CP14ModularCraftSlotPrototype>> AddSlots = new();

    //Icon

    //Inhand left right

    //Clothing

    //Inhand wielding
}
