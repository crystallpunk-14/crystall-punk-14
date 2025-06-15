using System.Linq;
using System.Text;
using Content.Shared._CP14.Skill.Components;
using Content.Shared._CP14.Skill.Prototypes;
using Content.Shared._CP14.Skill.Restrictions;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

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
        InitializeScanning();
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

        //Check if the skill is in the available skill trees
        if (!component.AvailableSkillTrees.Contains(skill.Tree))
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
    /// Obtaining all skills that are not prerequisites for other skills of this creature
    /// </summary>
    public HashSet<ProtoId<CP14SkillPrototype>> GetFrontierSkills(EntityUid target,
        CP14SkillStorageComponent? component = null)
    {
        var skills = new HashSet<ProtoId<CP14SkillPrototype>>();
        if (!Resolve(target, ref component, false))
            return skills;

        var frontier = component.LearnedSkills.ToHashSet();
        foreach (var skill in component.LearnedSkills)
        {
            if (!_proto.TryIndex(skill, out var indexedSkill))
                continue;

            if (HaveFreeSkill(target, skill))
                continue;

            foreach (var req in indexedSkill.Restrictions)
            {
                if (req is NeedPrerequisite prerequisite)
                {
                    if (frontier.Contains(prerequisite.Prerequisite))
                        frontier.Remove(prerequisite.Prerequisite);
                }
            }
        }

        return frontier;
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
        for(var i = component.LearnedSkills.Count - 1; i >= 0; i--)
        {
            if(HaveFreeSkill(target, component.LearnedSkills[i], component))
            {
                continue;
            }
            TryRemoveSkill(target, component.LearnedSkills[i], component);
        }
        return true;
    }

    /// <summary>
    /// Increases the number of memory points for a character, limited to a certain amount.
    /// </summary>
    public void AddMemoryPoints(EntityUid target, FixedPoint2 points, FixedPoint2 limit, CP14SkillStorageComponent? component = null)
    {
        if (!Resolve(target, ref component, false))
            return;

        component.ExperienceMaxCap = FixedPoint2.Min(component.ExperienceMaxCap + points, limit);
        Dirty(target, component);

        _popup.PopupEntity(Loc.GetString("cp14-skill-popup-added-points", ("count", points)), target, target);
    }

    /// <summary>
    /// Removes memory points. If a character has accumulated skills exceeding the new memory limit, random skills will be removed.
    /// </summary>
    public void RemoveMemoryPoints(EntityUid target, FixedPoint2 points, CP14SkillStorageComponent? component = null)
    {
        if (!Resolve(target, ref component, false))
            return;

        component.ExperienceMaxCap = FixedPoint2.Max(component.ExperienceMaxCap - points, 0);
        Dirty(target, component);

        _popup.PopupEntity(Loc.GetString("cp14-skill-popup-removed-points", ("count", points)), target, target);

        while (component.SkillsSumExperience > component.ExperienceMaxCap)
        {
            var frontier = GetFrontierSkills(target, component);
            if (frontier.Count == 0)
                break;

            //Randomly remove one of the frontier skills
            var skill = _random.Pick(frontier);
            TryRemoveSkill(target, skill, component);
        }
    }
}

[ByRefEvent]
public record struct CP14SkillLearnedEvent(ProtoId<CP14SkillPrototype> Skill, EntityUid User);
