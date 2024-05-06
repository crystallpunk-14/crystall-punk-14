
namespace Content.Server._CP14.MeleeWeapon;

/// <summary>
/// allows the object to become blunt with use
/// </summary>
[RegisterComponent, Access(typeof(CP14SharpeningSystem))]
public sealed partial class CP14SharpenedComponent : Component
{
    [DataField]
    public float Sharpness = 1f;

    [DataField]
    public float SharpnessDamageByHit = 0.01f;
}
