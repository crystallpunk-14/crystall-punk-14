using Content.Shared._CP14.Skill.Components;
using Content.Shared._CP14.Skill.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Skill;

public sealed partial class CP14SkillSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        InitializeAdmin();
    }

    public bool TryAddSkill(EntityUid target, ProtoId<CP14SkillPrototype> skill, CP14SkillStorageComponent? component = null)
    {
        if (!Resolve(target, ref component, false))
            return false;

        if (component.Skills.Contains(skill))
            return false;

        if (!_proto.TryIndex(skill, out var indexedSkill))
            return false;

        foreach (var effect in indexedSkill.Effects)
        {
            effect.AddSkill(EntityManager, target);
        }
        component.Skills.Add(skill);
        return true;
    }

    public bool TryRemoveSkill(EntityUid target, ProtoId<CP14SkillPrototype> skill, CP14SkillStorageComponent? component = null)
    {
        if (!Resolve(target, ref component, false))
            return false;

        if (!component.Skills.Remove(skill))
            return false;

        if (!_proto.TryIndex(skill, out var indexedSkill))
            return false;

        foreach (var effect in indexedSkill.Effects)
        {
            effect.RemoveSkill(EntityManager, target);
        }
        return true;
    }
}
