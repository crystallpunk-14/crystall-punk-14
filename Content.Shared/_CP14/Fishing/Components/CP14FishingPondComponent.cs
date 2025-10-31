using Content.Shared.EntityTable;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Fishing.Components;

/// <summary>
/// Component for fishing ponds
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(CP14SharedFishingSystem))]
public sealed partial class CP14FishingPondComponent : Component
{
    /// <summary>
    /// LootTable of loot that can be caught in this pond. Only first spawn will be caught
    /// </summary>
    [DataField]
    public ProtoId<EntityTablePrototype>? LootTable;
}
