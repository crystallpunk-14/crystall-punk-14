using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Cargo.Prototype;

[Prototype("storeFaction")]
public sealed partial class CP14StoreFactionPrototype : IPrototype
{
    [IdDataField, ViewVariables]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public LocId Name = string.Empty;

    [DataField]
    public LocId Desc = string.Empty;
}
