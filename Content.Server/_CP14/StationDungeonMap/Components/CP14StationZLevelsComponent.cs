using Content.Server._CP14.StationDungeonMap.EntitySystems;
using Robust.Shared.Map;
using Robust.Shared.Utility;

namespace Content.Server._CP14.StationDungeonMap.Components;

/// <summary>
/// allows you to control an array of maps linked to each other by z-levels
/// </summary>
[RegisterComponent]
public sealed partial class CP14ZLevelGroupComponent : Component
{
    [DataField]
    public Dictionary<int, MapId> Levels = new();
}

/// <summary>
///
/// </summary>
[RegisterComponent]
public sealed partial class CP14ZLevelElementComponent : Component
{
    [DataField]
    public Entity<CP14ZLevelGroupComponent>? Group = null;
}

