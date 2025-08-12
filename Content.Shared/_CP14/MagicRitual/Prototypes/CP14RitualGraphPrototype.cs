using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.MagicRitual.Prototypes;

[Prototype("ritualGraph")]
public sealed partial class CP14RitualGraphPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public LocId Name = string.Empty;
}
