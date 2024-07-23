using Content.Shared.Damage;

namespace Content.Server._CP14.MeleeWeapon.Components;

[RegisterComponent]
public sealed partial class CP14MeleeSelfDamageComponent : Component
{
    [DataField(required: true)]
    public DamageSpecifier DamageToSelf;
}
