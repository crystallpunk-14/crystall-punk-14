using Content.Shared._CP14.Skill.Prototypes;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Skill.Restrictions;

[ImplicitDataDefinitionForInheritors]
[MeansImplicitUse]
public abstract partial class CP14SkillRestriction
{
    /// <summary>
    /// If true - this skill won't be shown in skill tree if user doesn't meet this restriction
    /// </summary>
    public virtual bool HideFromUI => false;

    public abstract bool Check(IEntityManager entManager, EntityUid target);

    public abstract string GetDescription(IEntityManager entManager, IPrototypeManager protoManager);
}
