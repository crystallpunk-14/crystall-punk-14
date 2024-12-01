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
        var damageable = entManager.EnsureComponent<CP14DamageableModifierComponent>(start);

        damageable.Modifier *= Multiplier;
        entManager.Dirty(start);
    }
}
