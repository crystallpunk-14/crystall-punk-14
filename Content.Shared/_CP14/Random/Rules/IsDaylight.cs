using Content.Shared.Random.Rules;
using Robust.Shared.Map.Components;

namespace Content.Shared._CP14.Random.Rules;

/// <summary>
/// Checks whether there is a time of day on the current map, and whether the current time of day corresponds to the specified periods.
/// </summary>
public sealed partial class CP14TimePeriod : RulesRule
{
    [DataField]
    public float Threshold = 100f;

    public override bool Check(EntityManager entManager, EntityUid uid)
    {
        var transform = entManager.System<SharedTransformSystem>();

        var map = transform.GetMap(uid);
        if (!entManager.TryGetComponent<MapLightComponent>(map, out var light))
            return false;

        var lightColor = light.AmbientLightColor;
        var medium = lightColor.R + lightColor.G + lightColor.B / 3f;

        return medium > Threshold;
    }
}
