using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Fishing.FishingPool;

[RegisterComponent, NetworkedComponent]
public sealed partial class CP14FishingPoolComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public ProtoId<CP14FishingPoolLootTablePrototype> LootTable;
}
