using Content.Shared.Damage;
using Content.Shared.FixedPoint;

namespace Content.Server._CP14.Farming.Components;

/// <summary>
/// Gradually wastes the plant's resources, killing it if there aren't enough. The waste gradually increases over time, reflecting the "Old Age" of the plant
/// </summary>
[RegisterComponent, Access(typeof(CP14FarmingSystem))]
public sealed partial class CP14PlantFadingComponent : Component
{
    [DataField]
    public float ResourcePerMinute = 0f;

    [DataField]
    public DamageSpecifier FadeDamage = new()
    {
        DamageDict = new Dictionary<string, FixedPoint2>
        {
            { "Cellular", 0.05 },
        },
    };
}
