using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Skill.Restrictions;

[ImplicitDataDefinitionForInheritors]
[MeansImplicitUse]
public abstract partial class CP14SkillRestriction
{
    public abstract bool Check(IEntityManager entManager, EntityUid target);

    public abstract string GetDescription(IEntityManager entManager, IPrototypeManager protoManager);
}
