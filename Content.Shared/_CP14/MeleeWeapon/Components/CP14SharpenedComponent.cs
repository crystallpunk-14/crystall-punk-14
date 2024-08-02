using Content.Shared._CP14.MeleeWeapon.EntitySystems;

namespace Content.Shared._CP14.MeleeWeapon.Components;

/// <summary>
/// allows the object to become blunt with use
/// </summary>
[RegisterComponent, Access(typeof(CP14SharpeningSystem))]
public sealed partial class CP14SharpenedComponent : Component
{
    [DataField]
    public float Sharpness = 1f;

    [DataField]
    public float SharpnessDamageBy1Damage = 0.002f; //500 damage
}
