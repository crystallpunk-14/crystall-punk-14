using Content.Shared.Damage;

namespace Content.Server._CP14.Magic.Effects.Damage;

[RegisterComponent]
public sealed partial class CPCastEffectDamageComponent : Component
{
    [DataField(required: true)]
    public DamageSpecifier Damage = default!;

    [DataField]
    public bool IgnoreResistances;

    [DataField]
    public bool InterruptsDoAfters = true;
}
