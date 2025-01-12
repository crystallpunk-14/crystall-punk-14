using Content.Shared._CP14.DayCycle;
using Content.Shared._CP14.DayCycle.Components;
using Content.Shared._CP14.DayCycle.Prototypes;
using Content.Shared.Random.Rules;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Random.Rules;

/// <summary>
/// Checks whether there is a time of day on the current map, and whether the current time of day corresponds to the specified periods.
/// </summary>
public sealed partial class CP14TimePeriod : RulesRule
{
    [DataField] private List<ProtoId<CP14DayCyclePeriodPrototype>> Periods = new();

    public override bool Check(EntityManager entManager, EntityUid uid)
    {
        var transform = entManager.System<SharedTransformSystem>();

        var map = transform.GetMap(uid);
        if (!entManager.TryGetComponent<CP14DayCycleComponent>(map, out var dayCycle))
            return false;

        return dayCycle.CurrentPeriod is not null && Periods.Contains(dayCycle.CurrentPeriod.Value);
    }
}
