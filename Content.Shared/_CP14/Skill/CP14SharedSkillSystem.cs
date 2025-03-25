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

        SubscribeLocalEvent<CP14SkillStorageComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<CP14SkillStorageComponent> ent, ref MapInitEvent args)
    {
        //For debug add all skill trees to the player

        AddSkillTreeToPlayer(ent, "Pyrokinetic", ent.Comp);
        TryAddExperience(ent, "Pyrokinetic", 100, ent.Comp);

        AddSkillTreeToPlayer(ent, "Blacksmithing", ent.Comp);
        TryAddExperience(ent, "Blacksmithing", 100, ent.Comp);
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
        float exp,
        CP14SkillStorageComponent? component = null)
    {
        if (!Resolve(target, ref component, false))
            return false;

        if (!component.LearnProgress.ContainsKey(tree))
            return false;

        component.LearnProgress[tree] += exp;

        return true;
    }

    /// <summary>
    ///  Removes experience from the specified skill tree for the player.
    /// </summary>
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

        component.LearnProgress[tree] = FixedPoint2.Max(0, component.LearnProgress[tree] - exp);

        return true;
    }

    /// <summary>
    ///  Checks if the player can learn the specified skill.
    /// </summary>
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

        //Experience check
        if (!component.LearnProgress.TryGetValue(indexedSkill.Tree, out var currentExp))
            return false;
        if (currentExp < indexedSkill.LearnCost)
            return false;

        return true;
    }

    /// <summary>
    ///  Adds a skill tree to the player, allowing them to progress in it.
    /// </summary>
    public void AddSkillTreeToPlayer(EntityUid target,
        ProtoId<CP14SkillTreePrototype> tree,
        CP14SkillStorageComponent? component = null)
    {
        if (!Resolve(target, ref component, false))
            return;

        if (component.LearnProgress.ContainsKey(tree))
            return;

        component.LearnProgress[tree] = 0;
        Dirty(target, component);
        return;
    }

    /// <summary>
    ///  Removes a skill tree from the player, preventing them from progressing in it.
    /// </summary>
    public void RemoveSkillTreeFromPlayer(EntityUid target,
        ProtoId<CP14SkillTreePrototype> tree,
        CP14SkillStorageComponent? component = null)
    {
        if (!Resolve(target, ref component, false))
            return;

        if (!component.LearnProgress.Remove(tree))
            return;

        foreach (var skill in component.LearnedSkills)
        {
            if (!_proto.TryIndex(skill, out var indexedSkill))
                continue;

            if (indexedSkill.Tree == tree)
                TryRemoveSkill(target, skill, component);
        }

        Dirty(target, component);
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

        //if (!TryRemoveExperience(target, indexedSkill.Tree, indexedSkill.LearnCost, component))
        //    return false;

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
