using Content.Shared.Damage;

namespace Content.Server._CP14.MagicSpellStorage.Components;

/// <summary>
/// Causes damage to the Spell storage when spells from it are used
/// </summary>
[RegisterComponent, Access(typeof(CP14SpellStorageSystem))]
public sealed partial class CP14SpellStorageUseDamageComponent : Component
{
    /// <summary>
    /// the amount of damage this entity will take per unit manacost of the spell used
    /// </summary>
    [DataField(required: true)]
    public DamageSpecifier DamagePerMana = default!;
}
