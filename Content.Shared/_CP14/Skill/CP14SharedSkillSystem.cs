using System.Linq;
using System.Text;
using Content.Shared._CP14.Skill.Components;
using Content.Shared._CP14.Skill.Prototypes;
using Content.Shared._CP14.Skill.Restrictions;
using Content.Shared.FixedPoint;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Whitelist;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Shared._CP14.Skill;

public abstract partial class CP14SharedSkillSystem : EntitySystem
{
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private EntityQuery<CP14SkillStorageComponent> _skillStorageQuery;

    public override void Initialize()
    {
        base.Initialize();

        _skillStorageQuery = GetEntityQuery<CP14SkillStorageComponent>();

        SubscribeLocalEvent<CP14SkillStorageComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<CP14SkillPointConsumableComponent, UseInHandEvent>(OnInteracted);

        InitializeAdmin();
        InitializeChecks();
        InitializeScanning();
    }

    private void OnInteracted(Entity<CP14SkillPointConsumableComponent> ent, ref UseInHandEvent args)
    {
        if (!_timing.IsFirstTimePredicted)
            return;

        if (ent.Comp.Whitelist is null || !_whitelist.IsValid(ent.Comp.Whitelist, args.User))
            return;

        if (_net.IsServer)
            AddSkillPoints(args.User, ent.Comp.PointType, ent.Comp.Volume);

        var position = Transform(ent).Coordinates;

        //Client VFX
        if (_net.IsClient)
            SpawnAttachedTo(ent.Comp.ConsumeEffect, position);

        _audio.PlayPredicted(ent.Comp.ConsumeSound, position, args.User);

        PredictedQueueDel(ent.Owner);
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
    ///  Adds a skill tree to the player, allowing them to learn skills from it.
    /// </summary>
    public void AddSkillTree(EntityUid target,
        ProtoId<CP14SkillTreePrototype> tree,
        CP14SkillStorageComponent? component = null)
    {
        if (!Resolve(target, ref component))
            return;

        component.AvailableSkillTrees.Add(tree);
        DirtyField(target, component, nameof(CP14SkillStorageComponent.AvailableSkillTrees));
    }

    public void RemoveSkillTree(EntityUid target,
        ProtoId<CP14SkillTreePrototype> tree,
        CP14SkillStorageComponent? component = null)
    {
        if (!Resolve(target, ref component))
            return;

        component.AvailableSkillTrees.Remove(tree);
        DirtyField(target, component, nameof(CP14SkillStorageComponent.AvailableSkillTrees));
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

        if (!_proto.TryIndex(indexedSkill.Tree, out var indexedTree))
            return false;

        foreach (var effect in indexedSkill.Effects)
        {
            effect.AddSkill(EntityManager, target);
        }

        if (free)
            component.FreeLearnedSkills.Add(skill);
        else
        {
            if (component.SkillPoints.TryGetValue(indexedTree.SkillType, out var skillContainer))
            {
                skillContainer.Sum += indexedSkill.LearnCost;
            }
        }

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

        if (!_proto.TryIndex(indexedSkill.Tree, out var indexedTree))
            return false;

        foreach (var effect in indexedSkill.Effects)
        {
            effect.RemoveSkill(EntityManager, target);
        }

        if (!component.FreeLearnedSkills.Remove(skill))
        {
            if (component.SkillPoints.TryGetValue(indexedTree.SkillType, out var skillContainer))
            {
                skillContainer.Sum -= indexedSkill.LearnCost;
            }
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

        if (!_proto.TryIndex(skill.Tree, out var indexedTree))
            return false;

        //Already learned
        if (HaveSkill(target, skill, component))
            return false;

        //Check if the skill is in the available skill trees
        if (!component.AvailableSkillTrees.Contains(skill.Tree))
            return false;

        //Check skill points
        if (!component.SkillPoints.TryGetValue(indexedTree.SkillType, out var skillContainer))
            return false;

        if (skillContainer.Sum + skill.LearnCost > skillContainer.Max)
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

        for (var i = component.LearnedSkills.Count - 1; i >= 0; i--)
        {
            if (HaveFreeSkill(target, component.LearnedSkills[i], component))
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
    public void AddSkillPoints(EntityUid target,
        ProtoId<CP14SkillPointPrototype> type,
        FixedPoint2 points,
        FixedPoint2? limit = null,
        bool silent = false,
        CP14SkillStorageComponent? component = null)
    {
        if (!Resolve(target, ref component, false))
            return;

        if (!_proto.TryIndex(type, out var indexedType))
            return;

        if (!component.SkillPoints.TryGetValue(type, out var skillContainer))
        {
            skillContainer = new CP14SkillPointContainerEntry();
            component.SkillPoints[type] = skillContainer;
        }

        skillContainer.Max = limit is not null
            ? FixedPoint2.Min(skillContainer.Max + points, limit.Value)
            : skillContainer.Max + points;

        DirtyField(target, component, nameof(CP14SkillStorageComponent.SkillPoints));

        if (indexedType.GetPointPopup is not null && !silent)
            _popup.PopupPredicted(Loc.GetString(indexedType.GetPointPopup, ("count", points)), target, target);
    }

    /// <summary>
    /// Removes memory points. If a character has accumulated skills exceeding the new memory limit, random skills will be removed.
    /// </summary>
    public void RemoveSkillPoints(EntityUid target,
        ProtoId<CP14SkillPointPrototype> type,
        FixedPoint2 points,
        bool silent = false,
        CP14SkillStorageComponent? component = null)
    {
        if (!Resolve(target, ref component, false))
            return;

        if (!_proto.TryIndex(type, out var indexedType))
            return;

        if (!component.SkillPoints.TryGetValue(type, out var skillContainer))
            return;

        skillContainer.Max = FixedPoint2.Max(skillContainer.Max - points, 0);
        Dirty(target, component);

        if (indexedType.LosePointPopup is not null && !silent)
            _popup.PopupPredicted(Loc.GetString(indexedType.LosePointPopup, ("count", points)), target, target);

        while (skillContainer.Sum > skillContainer.Max)
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
