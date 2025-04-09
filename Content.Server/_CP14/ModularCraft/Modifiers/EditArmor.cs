using Content.Shared._CP14.ModularCraft;
using Content.Shared._CP14.ModularCraft.Components;
using Content.Shared.Armor;
using Content.Shared.Damage;

namespace Content.Server._CP14.ModularCraft.Modifiers;

public sealed partial class EditArmor : CP14ModularCraftModifier
{
    [DataField(required: true)]
    public DamageModifierSet Modifiers = new();

    public override void Effect(EntityManager entManager, Entity<CP14ModularCraftStartPointComponent> start, Entity<CP14ModularCraftPartComponent>? part)
    {
        if (!entManager.TryGetComponent<ArmorComponent>(start, out var armor))
            return;

        var armorSystem = entManager.System<SharedArmorSystem>();

        armorSystem.EditArmorCoefficients(start, Modifiers, armor);
    }
}
