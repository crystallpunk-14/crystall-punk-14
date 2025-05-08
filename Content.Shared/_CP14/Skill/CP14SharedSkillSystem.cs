using Content.Shared._CP14.Skill.Components;
using Content.Shared._CP14.Skill.Prototypes;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Skill;

public abstract partial class CP14SharedSkillSystem : EntitySystem
{

    public override void Initialize()
    {
        base.Initialize();

        InitializeAdmin();
    }

    /// <summary>
    /// Directly adds the skill to the player, bypassing any checks.
    /// </summary>
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

        if (indexedSkill.Effect is not null)
        {
            indexedSkill.Effect.AddSkill(EntityManager, target);
        }

        component.SkillsSumExperience += indexedSkill.LearnCost;

        component.LearnedSkills.Add(skill);
        Dirty(target, component);
        return true;
    }

    /// <summary>
    ///  Removes the skill from the player, bypassing any checks.
    /// </summary>
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

        if (indexedSkill.Effect is not null)
        {
            indexedSkill.Effect.RemoveSkill(EntityManager, target);
        }

        component.SkillsSumExperience -= indexedSkill.LearnCost;

        Dirty(target, component);
        return true;
    }

    /// <summary>
    ///  Checks if the player has the skill.
    /// </summary>
    public bool HaveSkill(EntityUid target,
        ProtoId<CP14SkillPrototype> skill,
        CP14SkillStorageComponent? component = null)
    {
        if (!Resolve(target, ref component, false))
            return false;

        return component.LearnedSkills.Contains(skill);
    }

    /// <summary>
    ///  Adds experience to the specified skill tree for the player.
    /// </summary>
    public bool TryAddExperience(EntityUid target,
        ProtoId<CP14SkillTreePrototype> tree,
        FixedPoint2 exp,
        CP14SkillStorageComponent? component = null)
    {
        if (!Resolve(target, ref component, false))
            return false;

        if (component.Progress.TryGetValue(tree, out var currentExp))
        {
            // If the tree already exists, add experience to it
            component.Progress[tree] = currentExp + exp;
        }
        else
        {
            // If the tree doesn't exist, initialize it with the experience
            component.Progress[tree] = exp;
        }

        Dirty(target, component);
        return true;
    }

    /// <summary>
    ///  Removes experience from the specified skill tree for the player.
    /// </summary>
    public bool TryRemoveExperience(EntityUid target,
        ProtoId<CP14SkillTreePrototype> tree,
        FixedPoint2 exp,
        CP14SkillStorageComponent? component = null)
    {
        if (!Resolve(target, ref component, false))
            return false;

        if (!component.Progress.TryGetValue(tree, out var currentExp))
            return false;

        if (currentExp < exp)
            return false;

        component.Progress[tree] = FixedPoint2.Max(0, component.Progress[tree] - exp);

        Dirty(target, component);
        return true;
    }

    /// <summary>
    ///  Checks if the player can learn the specified skill.
    /// </summary>
    public bool CanLearnSkill(EntityUid target,
        ProtoId<CP14SkillPrototype> skill,
        CP14SkillStorageComponent? component = null)
    {
        if (!_proto.TryIndex(skill, out var indexedSkill))
            return false;

        return CanLearnSkill(target, indexedSkill, component);
    }

    /// <summary>
    ///  Checks if the player can learn the specified skill.
    /// </summary>
    public bool CanLearnSkill(EntityUid target,
        CP14SkillPrototype skill,
        CP14SkillStorageComponent? component = null)
    {
        if (!Resolve(target, ref component, false))
            return false;

        if (!AllowedToLearn(target, skill, component))
            return false;

        //Experience check
        if (!component.Progress.TryGetValue(skill.Tree, out var currentExp))
            return false;
        if (currentExp < skill.LearnCost)
            return false;

        return true;
    }

    /// <summary>
    /// Is it allowed to learn this skill? The player may not have enough points to learn it, but has already met all the requirements to learn it.
    /// </summary>
    public bool AllowedToLearn(EntityUid target,
        CP14SkillPrototype skill,
        CP14SkillStorageComponent? component = null)
    {
        if (!Resolve(target, ref component, false))
            return false;

        //Already learned
        if (HaveSkill(target, skill, component))
            return false;

        //Check max cap
        if (component.SkillsSumExperience + skill.LearnCost >= component.ExperienceMaxCap)
            return false;

        //Restrictions check
        foreach (var req in skill.Restrictions)
        {
            if (!req.Check(EntityManager, target))
                return false;
        }

        return true;
    }

    /// <summary>
    ///  Tries to learn the specified skill for the player.
    /// </summary>
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

        if (!TryRemoveExperience(target, indexedSkill.Tree, indexedSkill.LearnCost, component))
            return false;

        if (!TryAddSkill(target, skill, component))
            return false;

        return false;
    }

    /// <summary>
    ///  Helper function to get the skill name for a given skill prototype.
    /// </summary>
    public string GetSkillName(ProtoId<CP14SkillPrototype> skill)
    {
        if (!_proto.TryIndex(skill, out var indexedSkill))
            return string.Empty;

        if (indexedSkill.Name != null)
            return Loc.GetString(indexedSkill.Name);

        if (indexedSkill.Effect != null)
            return indexedSkill.Effect.GetName(EntityManager, _proto) ?? string.Empty;

        return string.Empty;
    }

    /// <summary>
    ///  Helper function to get the skill description for a given skill prototype.
    /// </summary>
    public string GetSkillDescription(ProtoId<CP14SkillPrototype> skill)
    {
        if (!_proto.TryIndex(skill, out var indexedSkill))
            return string.Empty;

        if (indexedSkill.Desc != null)
            return Loc.GetString(indexedSkill.Desc);

        if (indexedSkill.Effect != null)
            return indexedSkill.Effect.GetDescription(EntityManager, _proto) ?? string.Empty;

        return string.Empty;
    }
}
