using Content.Shared._CP14.Skill.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Workbench.Requirements;

public sealed partial class SkillRequired : CP14WorkbenchCraftRequirement
{
    [DataField(required: true)]
    public List<ProtoId<CP14SkillPrototype>> Skills = new();

    public override bool CheckRequirement(EntityManager entManager,
        IPrototypeManager protoManager,
        HashSet<EntityUid> placedEntities)
    {
        return true;
    }
}
