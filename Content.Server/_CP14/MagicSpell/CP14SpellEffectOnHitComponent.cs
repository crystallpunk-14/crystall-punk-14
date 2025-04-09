using Content.Shared._CP14.MagicSpell.Spells;
using Content.Shared.Whitelist;

namespace Content.Server._CP14.MagicSpell;

/// <summary>
/// Component that allows an meleeWeapon to apply effects to other entities on melee attacks.
/// </summary>
[RegisterComponent]
public sealed partial class CP14SpellEffectOnHitComponent : Component
{
    [DataField(required: true, serverOnly: true)]
    public List<CP14SpellEffect> Effects = new();

    [DataField]
    public EntityWhitelist? Whitelist;

    [DataField]
    public float Prob = 1f;
}
