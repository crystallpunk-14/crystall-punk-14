using Content.Shared._CP14.ModularCraft;
using Content.Shared._CP14.ModularCraft.Components;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.ModularCraft.Modifiers;

public sealed partial class AddComponents : CP14ModularCraftModifier
{
    [DataField]
    public ComponentRegistry? Components;

    [DataField]
    public bool Override = false;

    public override void Effect(EntityManager entManager, Entity<CP14ModularCraftStartPointComponent> start, Entity<CP14ModularCraftPartComponent>? part)
    {
        if (Components is not null)
            entManager.AddComponents(start, Components, Override);
    }
}
