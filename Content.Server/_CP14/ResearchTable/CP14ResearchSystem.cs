using Content.Server.DoAfter;
using Content.Shared._CP14.ResearchTable;
using Content.Shared._CP14.Skill;
using Content.Shared._CP14.Skill.Components;
using Content.Shared._CP14.Skill.Prototypes;
using Content.Shared._CP14.Skill.Restrictions;
using Content.Shared.DoAfter;
using Content.Shared.UserInterface;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.ResearchTable;

public sealed class CP14ResearchSystem : CP14SharedResearchSystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly UserInterfaceSystem _userInterface = default!;
    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    [Dependency] private readonly AudioSystem _audio = default!;

    private IEnumerable<CP14SkillPrototype> _allSkills = [];

    public override void Initialize()
    {
        base.Initialize();

        _allSkills = _proto.EnumeratePrototypes<CP14SkillPrototype>();

        SubscribeLocalEvent<CP14ResearchTableComponent, BeforeActivatableUIOpenEvent>(OnBeforeUIOpen);
        SubscribeLocalEvent<CP14ResearchTableComponent, CP14ResearchMessage>(OnResearch);
        SubscribeLocalEvent<CP14ResearchTableComponent, CP14ResearchDoAfterEvent>(OnResearchEnd);

        SubscribeLocalEvent<PrototypesReloadedEventArgs>(OnReloadPrototypes);
    }

    private void OnReloadPrototypes(PrototypesReloadedEventArgs ev)
    {
        _allSkills = _proto.EnumeratePrototypes<CP14SkillPrototype>();
    }

    private void OnResearchEnd(Entity<CP14ResearchTableComponent> table, ref CP14ResearchDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        if (!_proto.TryIndex(args.Skill, out var indexedSkill))
            return;

        var placedEntities = _lookup.GetEntitiesInRange(Transform(table).Coordinates,
            table.Comp.ResearchRadius,
            LookupFlags.Uncontained);

        if (!CanResearch(indexedSkill, placedEntities, args.User))
            return;

        if (!TryComp<CP14SkillStorageComponent>(args.User, out var storage))
            return;
        if (storage.ResearchedSkills.Contains(args.Skill) || storage.LearnedSkills.Contains(args.Skill))
            return;
        storage.ResearchedSkills.Add(args.Skill);
        Dirty(args.User, storage);

        foreach (var restriction in indexedSkill.Restrictions)
        {
            switch (restriction)
            {
                case Researched researched:
                    foreach (var req in researched.Requirements)
                    {
                        req.PostCraft(EntityManager, _proto, placedEntities, args.User);
                    }
                    break;
            }
        }

        _audio.PlayPvs(table.Comp.ResearchSound, table);
        UpdateUI(table, args.User);
        args.Handled = true;
    }

    private void OnResearch(Entity<CP14ResearchTableComponent> ent, ref CP14ResearchMessage args)
    {
        if (!TryComp<CP14SkillStorageComponent>(args.Actor, out var storage))
            return;

        if (storage.ResearchedSkills.Contains(args.Skill) || storage.LearnedSkills.Contains(args.Skill))
            return;

        if (!_proto.TryIndex(args.Skill, out var indexedSkill))
            return;

        StartResearch(ent, args.Actor, indexedSkill);
    }

    private void OnBeforeUIOpen(Entity<CP14ResearchTableComponent> ent, ref BeforeActivatableUIOpenEvent args)
    {
        UpdateUI(ent, args.User);
    }

    private void UpdateUI(Entity<CP14ResearchTableComponent> entity, EntityUid user)
    {
        var placedEntities = _lookup.GetEntitiesInRange(Transform(entity).Coordinates, entity.Comp.ResearchRadius);

        if (!TryComp<CP14SkillStorageComponent>(user, out var storage))
            return;

        var researches = new List<CP14ResearchUiEntry>();
        foreach (var skill in _allSkills)
        {
            var researchable = false;
            var canCraft = true;
            var hidden = false;

            foreach (var restriction in skill.Restrictions)
            {
                if (storage.ResearchedSkills.Contains(skill) || storage.LearnedSkills.Contains(skill))
                    continue;

                switch (restriction)
                {
                    case SpeciesWhitelist speciesWhitelist: //We cant change species of our character, so hide it
                        if (!speciesWhitelist.Check(EntityManager, user, skill))
                            hidden = true;
                        break;

                    case NeedPrerequisite prerequisite:
                        if (!storage.ResearchedSkills.Contains(prerequisite.Prerequisite))
                            hidden = true;
                        break;

                    case Researched researched:
                        researchable = true;

                        foreach (var req in researched.Requirements)
                        {
                            if (!req.CheckRequirement(EntityManager, _proto, placedEntities, user))
                            {
                                canCraft = false;
                            }
                        }
                        break;
                }
            }

            if (!researchable || hidden)
                continue;

            var entry = new CP14ResearchUiEntry(skill, canCraft);

            researches.Add(entry);
        }

        _userInterface.SetUiState(entity.Owner, CP14ResearchTableUiKey.Key, new CP14ResearchTableUiState(researches));
    }

    private void StartResearch(Entity<CP14ResearchTableComponent> table, EntityUid user, CP14SkillPrototype skill)
    {
        var researchDoAfter = new CP14ResearchDoAfterEvent()
        {
            Skill = skill
        };

        var doAfterArgs = new DoAfterArgs(EntityManager,
            user,
            TimeSpan.FromSeconds(table.Comp.ResearchSpeed),
            researchDoAfter,
            table,
            table)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            NeedHand = true,
        };

        _doAfter.TryStartDoAfter(doAfterArgs);
        _audio.PlayPvs(table.Comp.ResearchSound, table);
    }

    private bool CanResearch(CP14SkillPrototype skill, HashSet<EntityUid> entities, EntityUid user)
    {
        foreach (var restriction in skill.Restrictions)
        {
            switch (restriction)
            {
                case Researched researched:
                    foreach (var req in researched.Requirements)
                    {
                        if (!req.CheckRequirement(EntityManager, _proto, entities, user))
                            return false;
                    }
                    break;
            }
        }

        return true;
    }
}
