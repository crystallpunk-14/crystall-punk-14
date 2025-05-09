using Content.Shared._CP14.Skill.Prototypes;
using Content.Shared.DoAfter;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._CP14.ResearchTable;

public abstract class CP14SharedResearchSystem : EntitySystem
{
}

[Serializable, NetSerializable]
public sealed partial class CP14ResearchDoAfterEvent : DoAfterEvent
{
    [DataField(required: true)]
    public ProtoId<CP14SkillPrototype> Recipe = default!;

    public override DoAfterEvent Clone() => this;
}
