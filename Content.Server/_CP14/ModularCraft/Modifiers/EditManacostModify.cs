using Content.Shared._CP14.ModularCraft;
using Content.Shared._CP14.ModularCraft.Components;
using Content.Shared._CP14.MagicManacostModify;
using Content.Shared._CP14.MagicRitual.Prototypes;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.ModularCraft.Modifiers;

public sealed partial class EditManacostModify : CP14ModularCraftModifier
{
    [DataField]
    public FixedPoint2 GlobalModifier = 1f;

    public override void Effect(EntityManager entManager, Entity<CP14ModularCraftStartPointComponent> start, Entity<CP14ModularCraftPartComponent>? part)
    {
        if (!entManager.TryGetComponent<CP14MagicManacostModifyComponent>(start, out var manacostModifyComp))
            return;

        manacostModifyComp.GlobalModifier += GlobalModifier;
    }
}
