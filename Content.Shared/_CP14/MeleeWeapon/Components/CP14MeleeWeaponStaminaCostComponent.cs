using Robust.Shared.GameStates;

namespace Content.Shared._CP14.MeleeWeapon.Components;

/// <summary>
/// Using this weapon damages the wearer's stamina.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class CP14MeleeWeaponStaminaCostComponent : Component
{
    [DataField]
    public float Stamina = 10f;
}
