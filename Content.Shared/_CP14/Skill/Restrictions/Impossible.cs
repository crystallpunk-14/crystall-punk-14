using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Skill.Restrictions;

public sealed partial class Impossible : CP14SkillRestriction
{
    public override bool Check(IEntityManager entManager, EntityUid target)
    {
        return false;
    }

    public override string GetDescription(IEntityManager entManager, IPrototypeManager protoManager)
    {
        return Loc.GetString("cp14-skill-req-impossible");
    }
}
