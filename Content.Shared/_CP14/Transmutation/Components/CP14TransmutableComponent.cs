using Content.Shared._CP14.Transmutation.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Transmutation.Components;

[RegisterComponent, Access(typeof(CP14TransmutationSystem))]
public sealed partial class CP14TransmutableComponent : Component
{
    [DataField]
    public Dictionary<ProtoId<CP14TransmutationPrototype>, EntProtoId> Entries = new();

    [DataField]
    public TimeSpan Time = TimeSpan.FromSeconds(3);
}
