using Content.Shared._CP14.MeleeWeapon.EntitySystems;
using Robust.Shared.GameStates;

namespace Content.Shared._CP14.MeleeWeapon.Components;

/// <summary>
/// allows the object to become blunt with use
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CP14SharpenedComponent : Component
{
    [DataField, AutoNetworkedField]
    public float Sharpness = 1f;

    [DataField]
    public float SharpnessDamageBy1Damage = 0.001f; //1000 damage
}
