using Content.Shared.Random.Rules;

namespace Content.Shared._CP14.Random.Rules;

/// <summary>
/// Returns true if the attached entity is in space.
/// </summary>
public sealed partial class IsDaylight : RulesRule
{
    public override bool Check(EntityManager entManager, EntityUid uid)
    {
        var transform = entManager.System<SharedTransformSystem>();

        // Not shared yet, use raw component data from map
        // var dayCycle = entManager.System<CP14DayCycleSystem>();

        //черт, нужны комиты из ветки фермерства
        return !Inverted;
    }
}
