using Content.Shared._CP14.Actions;
using Content.Shared.Damage;
using Robust.Shared.GameStates;

namespace Content.Shared._CP14.Armor;

/// <summary>
/// When the wearer takes damage, part of that damage is also taken by this item of clothing.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(CP14ArmorSystem))]
public sealed partial class CP14ArmorDamageAbsorptionComponent : Component
{
    [DataField]
    public float Absorption = 0.3f;
}
