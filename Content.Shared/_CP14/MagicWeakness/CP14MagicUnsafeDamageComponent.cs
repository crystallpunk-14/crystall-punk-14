using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;

namespace Content.Shared._CP14.MagicWeakness;

/// <summary>
/// imposes damage on excessive use of magic
/// </summary>
[RegisterComponent, NetworkedComponent]
[Access(typeof(CP14SharedMagicWeaknessSystem))]
public sealed partial class CP14MagicUnsafeDamageComponent : Component
{
    [DataField]
    public DamageSpecifier DamagePerEnergy = new()
    {
        DamageDict = new Dictionary<string, FixedPoint2>
        {
            {"CP14ManaDepletion", 0.8},
        },
    };
}
