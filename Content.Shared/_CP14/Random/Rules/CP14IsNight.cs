using Content.Shared._CP14.DayCycle;
using Content.Shared.Random.Rules;

namespace Content.Shared._CP14.Random.Rules;

/// <summary>
/// Checks whether there is a time of day on the current map, and whether the current time of day corresponds to the specified periods.
/// </summary>
public sealed partial class CP14IsNight : RulesRule
{
    public override bool Check(EntityManager entManager, EntityUid uid)
    {
        var transform = entManager.System<SharedTransformSystem>();
        var dayCycle = entManager.System<CP14DayCycleSystem>();

        var map = transform.GetMap(uid);

        if (map is null)
            return false;

        var isDay = dayCycle.IsDayNow(map.Value);

        return Inverted ? isDay : !isDay;
    }
}
