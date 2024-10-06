using Content.Server._CP14.StationDungeonMap.EntitySystems;
using Content.Shared._CP14.StationZLevels;
using Content.Shared.Destructible.Thresholds;

namespace Content.Server._CP14.StationDungeonMap.Components;

/// <summary>
/// Creates a chain of z-levels overloading with time
/// </summary>
[RegisterComponent, Access(typeof(CP14StationAbyssSystem))]
public sealed partial class CP14StationAbyssComponent : Component
{
    [DataField]
    public Dictionary<int, HashSet<CP14ZLevelPrototype>> Levels = new();

    [DataField]
    public TimeSpan MinReloadTime = TimeSpan.FromMinutes(5);

    [DataField]
    public TimeSpan MaxReloadTime = TimeSpan.FromMinutes(30);

    [DataField]
    public TimeSpan NextReloadTime = TimeSpan.Zero;

    // Telegraphy and delays

    [DataField]
    public TimeSpan PreReloadAlertTime = TimeSpan.FromSeconds(120f);

   //[DataField]
   //public AbyssState Status = AbyssState.Generating;
}

public enum AbyssState : byte
{
    Generating = 0,
    Waiting = 1,
    Destroying = 2,
}
