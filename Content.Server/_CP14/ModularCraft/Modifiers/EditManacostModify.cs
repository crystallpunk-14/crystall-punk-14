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
    public Dictionary<ProtoId<CP14MagicTypePrototype>, FixedPoint2> Modifiers = new();

    [DataField]
    public FixedPoint2 GlobalModifier = 1f;

    public override void Effect(EntityManager entManager, Entity<CP14ModularCraftStartPointComponent> start, Entity<CP14ModularCraftPartComponent>? part)
    {
        if (!entManager.TryGetComponent<CP14MagicManacostModifyComponent>(start, out var manacostModifyComp))
            return;

        foreach (var (magicType, modifier) in Modifiers)
        {
            if (manacostModifyComp.Modifiers.ContainsKey(magicType))
            {
                if (modifier >= 1f)
                    manacostModifyComp.Modifiers[magicType] += modifier - 1f;
                else
                    manacostModifyComp.Modifiers[magicType] -= 1f - modifier;
            }
            else
            {
                manacostModifyComp.Modifiers[magicType] = modifier;
            }
        }

        manacostModifyComp.GlobalModifier += GlobalModifier;
    }
}
