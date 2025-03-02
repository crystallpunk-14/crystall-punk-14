using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.MagicRitual.Prototypes;

/// <summary>
/// Represents a type of magic
/// </summary>
[Prototype("magicType")]
public sealed class CP14MagicTypePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public LocId Name = string.Empty;

    [DataField(required: true)]
    public Color Color = Color.White;

    [DataField]
    public EntProtoId? EssenceProto;
}
