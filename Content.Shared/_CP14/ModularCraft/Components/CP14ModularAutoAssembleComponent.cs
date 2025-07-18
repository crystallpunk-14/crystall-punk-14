using Content.Shared._CP14.ModularCraft.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.ModularCraft.Components;

/// <summary>
/// Adds all details to the item when initializing. This is useful for spawning modular items directly when mapping or as loot in demiplanes.
/// </summary>
[RegisterComponent, Access(typeof(CP14SharedModularCraftSystem))]
public sealed partial class CP14ModularCraftAutoAssembleComponent : Component
{
    [DataField]
    public List<EntProtoId> Details = new();
}
