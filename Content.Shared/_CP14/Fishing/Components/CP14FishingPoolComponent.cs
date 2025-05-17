using Content.Shared._CP14.Fishing.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Fishing.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class CP14FishingPoolComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public ProtoId<CP14FishingPoolLootTablePrototype> LootTable = "Default";
}
