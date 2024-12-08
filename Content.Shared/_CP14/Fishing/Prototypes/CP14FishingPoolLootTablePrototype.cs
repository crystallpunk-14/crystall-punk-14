using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Fishing.Prototypes;

[Prototype("CP14FishingPoolLootTable")]
public sealed class CP14FishingPoolLootTablePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; } = string.Empty;

    [DataField]
    public List<EntProtoId> Prototypes = [];
}
