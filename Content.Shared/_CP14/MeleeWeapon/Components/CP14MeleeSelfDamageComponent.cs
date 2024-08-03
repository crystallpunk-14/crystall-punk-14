using Content.Shared.Damage;

namespace Content.Shared._CP14.MeleeWeapon.Components;

[RegisterComponent]
public sealed partial class CP14MeleeSelfDamageComponent : Component
{
    [DataField]
    public DamageSpecifier DamageToSelf = new()
    {
        DamageDict = new()
        {
            { "Blunt", 1 },
        }
    };
}
