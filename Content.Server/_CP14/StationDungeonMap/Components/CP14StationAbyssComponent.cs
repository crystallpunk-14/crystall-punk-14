using Content.Server._CP14.StationDungeonMap.EntitySystems;
using Content.Shared._CP14.StationZLevels;

namespace Content.Server._CP14.StationDungeonMap.Components;

/// <summary>
/// Creates a chain of z-levels overloading with time
/// </summary>
[RegisterComponent, Access(typeof(CP14StationAbyssSystem))]
public sealed partial class CP14StationAbyssComponent : Component
{
    [DataField]
    public Dictionary<int, HashSet<CP14ZLevelPrototype>> Levels = new();
}
