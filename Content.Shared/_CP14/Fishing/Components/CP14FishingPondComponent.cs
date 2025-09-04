using System.Runtime.Serialization;
using Content.Shared.EntityTable;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Fishing.Components;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class CP14FishingPondComponent : Component
{
    [DataField]
    public ProtoId<EntityTablePrototype> LootTable;
}
