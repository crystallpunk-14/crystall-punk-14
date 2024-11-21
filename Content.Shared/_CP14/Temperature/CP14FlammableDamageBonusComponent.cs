using Content.Shared.Damage;

namespace Content.Shared._CP14.Temperature;

/// <summary>
/// Add bonus damage to melee attacks per flammable stack
/// </summary>
[RegisterComponent, Access(typeof(CP14SharedFireSpreadSystem))]
public sealed partial class CP14FlammableBonusDamageComponent : Component
{
    [DataField]
    public DamageSpecifier DamagePerStack = new()
    {
        DamageDict = new()
        {
            {"Heat", 0.3},
        }
    };
}
