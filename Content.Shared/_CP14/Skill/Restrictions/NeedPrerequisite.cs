using Content.Shared._CP14.Skill.Components;
using Content.Shared._CP14.Skill.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Skill.Restrictions;

public sealed partial class NeedPrerequisite : CP14SkillRestriction
{
    [DataField]
    public ProtoId<CP14SkillPrototype> Prerequisite = new();

    public override bool Check(IEntityManager entManager, EntityUid target)
    {
        if (!entManager.TryGetComponent<CP14SkillStorageComponent>(target, out var skillStorage))
            return false;

        var learned = skillStorage.LearnedSkills;
        return learned.Contains(Prerequisite);
    }

    public override string GetDescription(IEntityManager entManager, IPrototypeManager protoManager)
    {
        var skillSystem = entManager.System<CP14SharedSkillSystem>();

        return Loc.GetString("cp14-skill-req-prerequisite", ("name", skillSystem.GetSkillName(Prerequisite)));
    }
}
