using Content.Shared._CP14.ModularCraft.Components;
using Content.Shared.Damage;
using Content.Shared.Wieldable.Components;

namespace Content.Shared._CP14.ModularCraft.Modifiers;

public sealed partial class CP14ModularModifierModifyWieldedDamage : CP14ModularCraftModifier
{
    [DataField]
    public DamageSpecifier? BonusDamage;

    [DataField]
    public float? DamageMultiplier;

    public override void Effect(EntityManager entManager, Entity<CP14ModularCraftStartPointComponent> start)
    {
        if (!entManager.TryGetComponent<IncreaseDamageOnWieldComponent>(start, out var wield))
            return;

        if (BonusDamage is not null)
            wield.BonusDamage += BonusDamage;


        if (DamageMultiplier is not null)
            wield.BonusDamage *= DamageMultiplier.Value;
    }
}
