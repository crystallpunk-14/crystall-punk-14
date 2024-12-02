using Content.Shared._CP14.ModularCraft.Prototypes;
using Content.Shared._CP14.ModularCraft;
using Content.Shared._CP14.ModularCraft.Components;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.ModularCraft.Modifiers;

public sealed partial class AddNewSlots : CP14ModularCraftModifier
{
    [DataField]
    public List<ProtoId<CP14ModularCraftSlotPrototype>> Slots;

    public override void Effect(EntityManager entManager, Entity<CP14ModularCraftStartPointComponent> start, Entity<CP14ModularCraftPartComponent>? part)
    {
        foreach (var slot in Slots)
        {
            start.Comp.FreeSlots.Add(slot);
        }
    }
}
