using Content.Shared._CP14.Skill.Components;
using Content.Shared._CP14.Skill.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Skill.Restrictions;

public sealed partial class Impossible : CP14SkillRestriction
{
    public override bool Check(IEntityManager entManager, EntityUid target, CP14SkillPrototype skill)
    {
        return false;
    }

    public override string GetDescription(IEntityManager entManager, IPrototypeManager protoManager)
    {
        return Loc.GetString("cp14-skill-req-impossible");
    }
}
