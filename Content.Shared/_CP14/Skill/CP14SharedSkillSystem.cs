using Content.Shared._CP14.Skill.Components;
using Content.Shared._CP14.Skill.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Skill;

public abstract partial class CP14SharedSkillSystem : EntitySystem
{

    public override void Initialize()
    {
        base.Initialize();

        InitializeAdmin();
    }

    public bool TryAddSkill(EntityUid target,
        ProtoId<CP14SkillPrototype> skill,
        CP14SkillStorageComponent? component = null)
    {
        if (!Resolve(target, ref component, false))
            return false;

        if (component.LearnedSkills.Contains(skill))
            return false;

        if (!_proto.TryIndex(skill, out var indexedSkill))
            return false;

        foreach (var effect in indexedSkill.Effects)
        {
            effect.AddSkill(EntityManager, target);
        }

        component.LearnedSkills.Add(skill);
        Dirty(target, component);
        return true;
    }

    public bool TryRemoveSkill(EntityUid target,
        ProtoId<CP14SkillPrototype> skill,
        CP14SkillStorageComponent? component = null)
    {
        if (!Resolve(target, ref component, false))
            return false;

        if (!component.LearnedSkills.Remove(skill))
            return false;

        if (!_proto.TryIndex(skill, out var indexedSkill))
            return false;

        foreach (var effect in indexedSkill.Effects)
        {
            effect.RemoveSkill(EntityManager, target);
        }

        Dirty(target, component);
        return true;
    }

    public bool HaveSkill(EntityUid target,
        ProtoId<CP14SkillPrototype> skill,
        CP14SkillStorageComponent? component = null)
    {
        if (!Resolve(target, ref component, false))
            return false;

        return component.LearnedSkills.Contains(skill);
    }

    public void AddExperience(EntityUid target,
        ProtoId<CP14SkillTreePrototype> tree,
        float exp,
        CP14SkillStorageComponent? component = null)
    {
        if (!Resolve(target, ref component, false))
            return;

        if (!component.LearnProgress.TryAdd(tree, exp))
        {
            component.LearnProgress[tree] += exp;
        }
    }

    public bool TryRemoveExperience(EntityUid target,
        ProtoId<CP14SkillTreePrototype> tree,
        float exp,
        CP14SkillStorageComponent? component = null)
    {
        if (!Resolve(target, ref component, false))
            return false;

        if (!component.LearnProgress.TryGetValue(tree, out var currentExp))
            return false;

        if (currentExp < exp)
            return false;

        component.LearnProgress[tree] -= exp;
        if (component.LearnProgress[tree] <= 0)
        {
            component.LearnProgress.Remove(tree);
        }
        return true;
    }

    public bool CanLearnSkill(EntityUid target,
        ProtoId<CP14SkillPrototype> skill,
        CP14SkillStorageComponent? component = null)
    {
        if (!Resolve(target, ref component, false))
            return false;

        if (!_proto.TryIndex(skill, out var indexedSkill))
            return false;

        //Already learned
        if (HaveSkill(target, skill, component))
            return false;

        //Prerequisite check
        foreach (var prerequisite in indexedSkill.Prerequisites)
        {
            if (!HaveSkill(target, prerequisite, component))
                return false;
        }

        ////Experience check
        //if (!component.LearnProgress.TryGetValue(indexedSkill.Tree, out var currentExp))
        //    return false;
        //if (currentExp < indexedSkill.LearnCost)
        //    return false;

        return true;
    }

    public bool TryLearnSkill(EntityUid target,
        ProtoId<CP14SkillPrototype> skill,
        CP14SkillStorageComponent? component = null)
    {
        if (!Resolve(target, ref component, false))
            return false;

        if (!_proto.TryIndex(skill, out var indexedSkill))
            return false;

        if (!CanLearnSkill(target, skill, component))
            return false;

        //if (!TryRemoveExperience(target, indexedSkill.Tree, indexedSkill.LearnCost, component))
        //    return false;

        if (!TryAddSkill(target, skill, component))
            return false;

        return false;
    }
}
