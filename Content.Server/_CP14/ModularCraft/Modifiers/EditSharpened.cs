using Content.Shared._CP14.MeleeWeapon.Components;
using Content.Shared._CP14.ModularCraft;
using Content.Shared._CP14.ModularCraft.Components;

namespace Content.Server._CP14.ModularCraft.Modifiers;

public sealed partial class EditSharpened : CP14ModularCraftModifier
{
    [DataField]
    public float SharpnessDamageMultiplier = 1f;
    public override void Effect(EntityManager entManager, Entity<CP14ModularCraftStartPointComponent> start, Entity<CP14ModularCraftPartComponent>? part)
    {
        if (!entManager.TryGetComponent<CP14SharpenedComponent>(start, out var sharpened))
            return;

        sharpened.SharpnessDamageBy1Damage *= SharpnessDamageMultiplier;
    }
}
