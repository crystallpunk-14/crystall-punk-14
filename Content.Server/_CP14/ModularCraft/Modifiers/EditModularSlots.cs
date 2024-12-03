using Content.Shared._CP14.ModularCraft;
using Content.Shared._CP14.ModularCraft.Components;
using Content.Shared._CP14.ModularCraft.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.ModularCraft.Modifiers;

public sealed partial class EditModularSlots : CP14ModularCraftModifier
{
    [DataField]
    public HashSet<ProtoId<CP14ModularCraftSlotPrototype>> AddSlots = new();

    [DataField]
    public HashSet<ProtoId<CP14ModularCraftSlotPrototype>> RemoveSlots = new();

    public override void Effect(EntityManager entManager, Entity<CP14ModularCraftStartPointComponent> start, Entity<CP14ModularCraftPartComponent>? part)
    {
        start.Comp.FreeSlots.AddRange(AddSlots);
        foreach (var slot in RemoveSlots)
        {
            if (start.Comp.FreeSlots.Contains(slot))
                start.Comp.FreeSlots.Remove(slot);
        }
    }
}
