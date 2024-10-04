using Content.Shared._CP14.DayCycle;
using Content.Shared._CP14.DayCycle.Components;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.MagicRitual.Requirements;

/// <summary>
/// Requires specific daytime
/// </summary>
public sealed partial class RequiredTime : CP14RitualRequirement
{
    [DataField]
    public ProtoId<CP14DayCyclePeriodPrototype> TimePeriod;

    public override string? GetGuidebookRequirementDescription(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        if (!prototype.TryIndex(TimePeriod, out var indexed))
            return null;

        return Loc.GetString("cp14-ritual-required-time", ("period", Loc.GetString(indexed.Name)));
    }

    public override bool Check(EntityManager entManager, Entity<CP14MagicRitualPhaseComponent> phaseEnt, float stability)
    {
        var transform = entManager.System<SharedTransformSystem>();
        var map = transform.GetMap(phaseEnt.Owner);

        if (!entManager.TryGetComponent<CP14DayCycleComponent>(map, out var dayCycle))
            return false;

        return TimePeriod == dayCycle.CurrentPeriod;
    }
}
