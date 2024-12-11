using Content.Shared.Damage;
using Content.Shared.FixedPoint;

namespace Content.Server._CP14.MapDamage;

/// <summary>
/// The map deals damage to entities that are on it (not on the grid)
/// </summary>
[RegisterComponent]
public sealed partial class CP14MapDamageComponent : Component
{
    //Damage every second
    [DataField]
    public DamageSpecifier Damage = new()
    {
        DamageDict = new Dictionary<string, FixedPoint2>()
        {
            {"Asphyxiation", 10}
        }
    };

    [DataField]
    public float StaminaDamage = 7f;
}
