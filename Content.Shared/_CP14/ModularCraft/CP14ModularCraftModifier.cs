using Content.Shared._CP14.ModularCraft.Components;
using JetBrains.Annotations;

namespace Content.Shared._CP14.ModularCraft;

[ImplicitDataDefinitionForInheritors]
[MeansImplicitUse]
public abstract partial class CP14ModularCraftModifier
{
    public abstract void Effect(EntityManager entManager, Entity<CP14ModularCraftStartPointComponent> start, Entity<CP14ModularCraftPartComponent>? part);
}
