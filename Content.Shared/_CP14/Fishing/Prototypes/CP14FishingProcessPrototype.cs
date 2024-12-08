using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Fishing.Prototypes;

[Prototype("CP14FishingProcess")]
public sealed class CP14FishingProcessPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; } = string.Empty;

    [DataField, ViewVariables]
    public EntProtoId LootProtoId;
}
