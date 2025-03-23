using JetBrains.Annotations;

namespace Content.Shared._CP14.Skill.Specials;

[ImplicitDataDefinitionForInheritors]
[MeansImplicitUse]
public abstract partial class CP14SkillEffect
{
    public abstract void AddSkill(EntityManager entManager, EntityUid target);
}
