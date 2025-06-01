using System.Linq;
using System.Text;
using Content.Shared._CP14.Skill.Components;
using Content.Shared._CP14.Skill.Prototypes;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Skill;

public abstract partial class CP14SharedSkillSystem : EntitySystem
{
    private EntityQuery<CP14SkillStorageComponent> _skillStorageQuery = default!;

    public override void Initialize()
    {
        base.Initialize();

        _skillStorageQuery = GetEntityQuery<CP14SkillStorageComponent>();

        SubscribeLocalEvent<CP14SkillStorageComponent, MapInitEvent>(OnMapInit);

        InitializeAdmin();
        InitializeChecks();
    }

    private void OnMapInit(Entity<CP14SkillStorageComponent> ent, ref MapInitEvent args)
    {
        //If at initialization we have any skill records, we automatically give them to this entity

        var free = ent.Comp.FreeLearnedSkills.ToList();
        var learned = ent.Comp.LearnedSkills.ToList();

        ent.Comp.FreeLearnedSkills.Clear();
        ent.Comp.LearnedSkills.Clear();

        foreach (var skill in free)
        {
            TryAddSkill(ent.Owner, skill, ent.Comp, true);
        }

        foreach (var skill in learned)
        {
            TryAddSkill(ent.Owner, skill, ent.Comp);
        }
    }

    /// <summary>
    /// Directly adds the skill to the player, bypassing any checks.
    /// </summary>
    public bool TryAddSkill(EntityUid target,
        ProtoId<CP14SkillPrototype> skill,
        CP14SkillStorageComponent? component = null,
        bool free = false)
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

        if (free)
            component.FreeLearnedSkills.Add(skill);
        else
            component.SkillsSumExperience += indexedSkill.LearnCost;

        component.LearnedSkills.Add(skill);
        Dirty(target, component);

        var learnEv = new CP14SkillLearnedEvent(skill, target);
        RaiseLocalEvent(target, ref learnEv);

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

        foreach (var effect in indexedSkill.Effects)
        {
            effect.RemoveSkill(EntityManager, target);
        }

        if (!component.FreeLearnedSkills.Remove(skill))
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

    public bool HaveFreeSkill(EntityUid target,
        ProtoId<CP14SkillPrototype> skill,
        CP14SkillStorageComponent? component = null)
    {
        if (!Resolve(target, ref component, false))
            return false;

        return component.FreeLearnedSkills.Contains(skill);
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
        if (component.SkillsSumExperience + skill.LearnCost > component.ExperienceMaxCap)
            return false;

        //Restrictions check
        foreach (var req in skill.Restrictions)
        {
            if (!req.Check(EntityManager, target, skill))
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

        if (!CanLearnSkill(target, skill, component))
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

        if (indexedSkill.Name is not null)
            return Loc.GetString(indexedSkill.Name);

        if (indexedSkill.Effects.Count > 0)
            return indexedSkill.Effects.First().GetName(EntityManager, _proto) ?? string.Empty;

        return string.Empty;
    }

    /// <summary>
    ///  Helper function to get the skill description for a given skill prototype.
    /// </summary>
    public string GetSkillDescription(ProtoId<CP14SkillPrototype> skill)
    {
        if (!_proto.TryIndex(skill, out var indexedSkill))
            return string.Empty;

        if (indexedSkill.Desc is not null)
            return Loc.GetString(indexedSkill.Desc);

        var sb = new StringBuilder();

        foreach (var effect in indexedSkill.Effects)
        {
            sb.Append(effect.GetDescription(EntityManager, _proto, skill) + "\n");
        }

        return sb.ToString();
    }
    /// <summary>
    ///  Helper function to reset skills to only learned skills
    /// </summary>
    public bool TryResetSkills(EntityUid target,
        CP14SkillStorageComponent? component = null)
    {
        if (!Resolve(target, ref component, false))
        {
            return false;
        }

        foreach (var skill in from skill in component.LearnedSkills
                              where !HaveFreeSkill(target, skill, component)
                              select skill)
        {
            TryRemoveSkill(target, skill, component);
        }

        component.LearnedSkills = component.FreeLearnedSkills;
        component.SkillsSumExperience = 0;
        return true;
    }
}

[ByRefEvent]
public record struct CP14SkillLearnedEvent(ProtoId<CP14SkillPrototype> Skill, EntityUid User);
