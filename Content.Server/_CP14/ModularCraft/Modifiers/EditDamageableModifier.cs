using Content.Shared._CP14.Damageable;
using Content.Shared._CP14.ModularCraft;
using Content.Shared._CP14.ModularCraft.Components;

namespace Content.Server._CP14.ModularCraft.Modifiers;

public sealed partial class EditDamageableModifier : CP14ModularCraftModifier
{
    [DataField(required: true)]
    public float Multiplier = 1f;

    public override void Effect(EntityManager entManager, Entity<CP14ModularCraftStartPointComponent> start, Entity<CP14ModularCraftPartComponent>? part)
    {
        if (!entManager.TryGetComponent<CP14DamageableModifierComponent>(start, out var damageable))
            return;

        damageable.Modifier *= Multiplier;
        entManager.Dirty(start);
    }
}
