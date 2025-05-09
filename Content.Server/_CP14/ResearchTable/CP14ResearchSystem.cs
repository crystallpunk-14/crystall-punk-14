using Content.Shared._CP14.ResearchTable;
using Content.Shared._CP14.Skill;
using Content.Shared._CP14.Skill.Prototypes;
using Content.Shared._CP14.Skill.Restrictions;
using Content.Shared.UserInterface;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.ResearchTable;

public sealed class CP14ResearchSystem : CP14SharedResearchSystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly UserInterfaceSystem _userInterface = default!;

    private IEnumerable<CP14SkillPrototype> _allSkills = [];

    public override void Initialize()
    {
        base.Initialize();

        _allSkills = _proto.EnumeratePrototypes<CP14SkillPrototype>();

        SubscribeLocalEvent<CP14ResearchTableComponent, BeforeActivatableUIOpenEvent>(OnBeforeUIOpen);
    }

    private void OnBeforeUIOpen(Entity<CP14ResearchTableComponent> ent, ref BeforeActivatableUIOpenEvent args)
    {
        UpdateUI(ent, args.User);
    }

    private void UpdateUI(Entity<CP14ResearchTableComponent> entity, EntityUid user)
    {
        var placedEntities = _lookup.GetEntitiesInRange(Transform(entity).Coordinates, entity.Comp.ResearchRadius);

        var researches = new List<CP14ResearchUiEntry>();
        foreach (var skill in _allSkills)
        {
            var researchable = false;
            var canCraft = true;

            foreach (var restriction in skill.Restrictions)
            {
                switch (restriction)
                {
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

            if (!researchable)
                continue;

            var entry = new CP14ResearchUiEntry(skill, canCraft);

            researches.Add(entry);
        }

        _userInterface.SetUiState(entity.Owner, CP14ResearchTableUiKey.Key, new CP14ResearchTableUiState(researches));
    }
}
