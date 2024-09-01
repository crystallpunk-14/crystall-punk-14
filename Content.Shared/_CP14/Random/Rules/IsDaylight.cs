using Content.Shared._CP14.DayCycle;
using Content.Shared.Random.Rules;

namespace Content.Shared._CP14.Random.Rules;

/// <summary>
/// Returns true if the attached entity is in space.
/// </summary>
public sealed partial class IsDaylight : RulesRule
{
    public override bool Check(EntityManager entManager, EntityUid uid)
    {
        var dayCycle = entManager.System<CP14SharedDayCycleSystem>();

        if (Inverted)
            return !dayCycle.TryDaylightThere(uid, true);
        else
            return dayCycle.TryDaylightThere(uid, true);
    }
}
