using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Fishing;

[Prototype("CP14FishingProcess")]
public sealed class CP14FishingProcessPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; } = string.Empty;

    [DataField, ViewVariables]
    public float Size;

    [DataField, ViewVariables]
    public float Gravity;

    [DataField, ViewVariables]
    public float PlayerSize;

    [DataField, ViewVariables]
    public float LootSize;

    [DataField, ViewVariables]
    public EntProtoId LootProtoId;
}
