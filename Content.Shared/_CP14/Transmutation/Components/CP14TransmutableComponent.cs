using Content.Shared._CP14.Skill.Prototypes;
using Content.Shared._CP14.Transmutation.Prototypes;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Transmutation.Components;

[RegisterComponent, Access(typeof(CP14TransmutationSystem))]
public sealed partial class CP14TransmutableComponent : Component
{
    [DataField]
    public Dictionary<ProtoId<CP14TransmutationPrototype>, EntProtoId> Entries = new();

    [DataField]
    public FixedPoint2 Cost = 1f;

    [DataField(required: true)]
    public ProtoId<CP14SkillPointPrototype>? Resource;
}
