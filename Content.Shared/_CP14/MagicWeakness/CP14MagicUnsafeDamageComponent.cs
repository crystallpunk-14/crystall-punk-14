using Content.Shared.Damage;
using Content.Shared.FixedPoint;

namespace Content.Shared._CP14.MagicWeakness;

/// <summary>
/// imposes damage on excessive use of magic
/// </summary>
[RegisterComponent, Access(typeof(CP14MagicWeaknessSystem))]
public sealed partial class CP14MagicUnsafeDamageComponent : Component
{
    [DataField]
    public DamageSpecifier DamagePerEnergy = new()
    {
        DamageDict = new Dictionary<string, FixedPoint2>
        {
            {"Blunt", 0.5},
            {"Heat", 0.5},
        },
    };
}
