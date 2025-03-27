using Content.Shared._CP14.Skill.Components;
using Content.Shared._CP14.Skill.Prototypes;
using Content.Shared.Bed.Sleep;
using Content.Shared.Examine;
using Content.Shared.FixedPoint;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Skill;

public abstract partial class CP14SharedSkillSystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly ExamineSystemShared _examine = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;

    public void GiveExperienceInRadius(EntityCoordinates position, ProtoId<CP14SkillTreePrototype> tree, FixedPoint2 points, float radius = 5)
    {
        var entities = _lookup.GetEntitiesInRange<CP14SkillStorageComponent>(position, radius, LookupFlags.Uncontained);

        foreach (var ent in entities)
        {
            //Cant learn if the position is not in range or obstructed
            if (!_examine.InRangeUnOccluded(ent, position, radius))
                continue;

            //Cant learn when dead
            if (TryComp<MobStateComponent>(ent, out var mobState) && !_mobState.IsAlive(ent, mobState))
                continue;

            //Cant learn if the entity is sleeping
            if (HasComp<SleepingComponent>(ent))
                continue;

            TryAddExperience(ent, tree, points);
        }
    }

    public void GiveExperienceInRadius(EntityUid uid,
        ProtoId<CP14SkillTreePrototype> tree,
        FixedPoint2 points,
        float radius = 5)
    {
        GiveExperienceInRadius(Transform(uid).Coordinates, tree, points, radius);
    }
}
