using Content.Shared._CP14.Transmutation.Prototypes;
using Content.Shared.StatusIcon;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Vampire;

[Prototype("cp14VampireFaction")]
public sealed partial class CP14VampireFactionPrototype : IPrototype
{
    [IdDataField] public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public LocId Name = string.Empty;

    [DataField(required: true)]
    public ProtoId<FactionIconPrototype> FactionIcon;

    [DataField(required: true)]
    public ProtoId<CP14TransmutationPrototype>? TransmutationMethod;

    [DataField(required: true)]
    public string SingletonTeleportKey = string.Empty;

    [DataField(required: true)]
    public EntProtoId? MotherTreeProto;
}
