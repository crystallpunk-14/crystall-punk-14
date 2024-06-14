using Content.Shared._CP14.Skills.Components;
using Content.Shared._CP14.Skills.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Shared._CP14.Skills;

public partial class SharedCP14SkillSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;

    public bool HasEnoughSkillToUse(EntityUid user, EntityUid target, out List<ProtoId<CP14SkillPrototype>> missingSkills)
    {
        missingSkills = new();
        if (!TryComp<CP14SkillRequirementComponent>(target, out var requirement) || requirement.RequiredSkills.Count == 0)
            return true;

        if (!TryComp<CP14SkillsStorageComponent>(user, out var skillStorage))
        {
            missingSkills = requirement.RequiredSkills;
            return false;
        }

        var success = requirement.NeedAll;
        foreach (var skill in requirement.RequiredSkills)
        {
            var hasSkill = skillStorage.Skills.Contains(skill);

            if (requirement.NeedAll && !hasSkill)
            {
                missingSkills.Add(skill);
                success = false;
            }
            else if (!requirement.NeedAll && hasSkill)
            {
                return true;
            }
            else if (!requirement.NeedAll && !hasSkill)
            {
                missingSkills.Add(skill);
            }
        }

        return success;
    }

    public bool TryLearnSkill(EntityUid uid, ProtoId<CP14SkillPrototype> skill)
    {
        if (!TryComp<CP14SkillsStorageComponent>(uid, out var skillStorage))
            return false;

        if (skillStorage.Skills.Contains(skill))
            return false;

        skillStorage.Skills.Add(skill);
        return true;
    }

    public bool TryForgotSkill(EntityUid uid, ProtoId<CP14SkillPrototype> skill)
    {
        if (!TryComp<CP14SkillsStorageComponent>(uid, out var skillStorage))
            return false;

        if (!skillStorage.Skills.Contains(skill))
            return false;

        skillStorage.Skills.Remove(skill);
        return true;
    }
}
