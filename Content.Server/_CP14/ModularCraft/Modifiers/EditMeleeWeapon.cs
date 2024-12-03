using Content.Shared._CP14.ModularCraft;
using Content.Shared._CP14.ModularCraft.Components;
using Content.Shared.Damage;
using Content.Shared.Weapons.Melee;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.ModularCraft.Modifiers;

public sealed partial class EditMeleeWeapon : CP14ModularCraftModifier
{
    [DataField]
    public EntProtoId? NewAnimation;

    [DataField]
    public EntProtoId? NewWideAnimation;

    [DataField]
    public float? AngleMultiplier;

    [DataField]
    public DamageSpecifier? BonusDamage;

    [DataField]
    public float? DamageMultiplier;

    [DataField]
    public float? AttackRateMultiplier;

    [DataField]
    public float? BonusRange;

    [DataField]
    public bool? ResetOnHandSelected;

    public override void Effect(EntityManager entManager, Entity<CP14ModularCraftStartPointComponent> start, Entity<CP14ModularCraftPartComponent>? part)
    {
        if (!entManager.TryGetComponent<MeleeWeaponComponent>(start, out var melee))
            return;

        if (NewAnimation is not null)
            melee.Animation = NewAnimation.Value;

        if (NewWideAnimation is not null)
            melee.WideAnimation = NewWideAnimation.Value;

        if (AngleMultiplier is not null)
            melee.Angle = Angle.FromDegrees(melee.Angle.Degrees * AngleMultiplier.Value);

        if (BonusDamage is not null)
            melee.Damage += BonusDamage;

        if (DamageMultiplier is not null)
            melee.Damage *= DamageMultiplier.Value;

        if (AttackRateMultiplier is not null)
            melee.AttackRate *= AttackRateMultiplier.Value;

        if (BonusRange is not null)
            melee.Range += BonusRange.Value;

        if (ResetOnHandSelected is not null)
            melee.ResetOnHandSelected = ResetOnHandSelected.Value;
    }
}
