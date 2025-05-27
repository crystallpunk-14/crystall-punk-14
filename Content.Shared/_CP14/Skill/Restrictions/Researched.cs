using Content.Shared._CP14.Skill.Components;
using Content.Shared._CP14.Skill.Prototypes;
using Content.Shared._CP14.Workbench;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Skill.Restrictions;

public sealed partial class Researched : CP14SkillRestriction
{
    [DataField(required: true)]
    public List<CP14WorkbenchCraftRequirement> Requirements = new();

    public override bool Check(IEntityManager entManager, EntityUid target, CP14SkillPrototype skill)
    {
        if (!entManager.TryGetComponent<CP14SkillStorageComponent>(target, out var skillStorage))
            return false;

        var learned = skillStorage.ResearchedSkills;
        return learned.Contains(skill);
    }

    public override string GetDescription(IEntityManager entManager, IPrototypeManager protoManager)
    {
        return Loc.GetString("cp14-skill-req-researched");
    }
}
