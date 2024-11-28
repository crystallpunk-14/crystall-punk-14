using Content.Shared._CP14.ModularCraft;
using Content.Shared._CP14.ModularCraft.Components;
using Content.Shared.Damage;
using Content.Shared.Wieldable.Components;

namespace Content.Server._CP14.ModularCraft.Modifiers;

public sealed partial class EditIncreaseDamageOnWield : CP14ModularCraftModifier
{
    [DataField]
    public DamageSpecifier? BonusDamage;

    [DataField]
    public float? DamageMultiplier;

    public override void Effect(EntityManager entManager, Entity<CP14ModularCraftStartPointComponent> start, Entity<CP14ModularCraftPartComponent>? part)
    {
        if (!entManager.TryGetComponent<IncreaseDamageOnWieldComponent>(start, out var wield))
            return;

        if (BonusDamage is not null)
            wield.BonusDamage += BonusDamage;

        if (DamageMultiplier is not null)
            wield.BonusDamage *= DamageMultiplier.Value;
    }

}
