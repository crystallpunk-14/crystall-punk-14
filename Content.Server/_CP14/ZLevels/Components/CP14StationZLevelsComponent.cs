using Content.Server._CP14.ZLevels.Commands;
using Content.Server._CP14.ZLevels.EntitySystems;
using Robust.Shared.Map;
using Robust.Shared.Utility;

namespace Content.Server._CP14.ZLevels.Components;

/// <summary>
/// Initializes the z-level system by creating a series of linked maps
/// </summary>
[RegisterComponent, Access(typeof(CP14StationZLevelsSystem), typeof(CP14CombineMapsIntoZLevelsCommand))]
public sealed partial class CP14StationZLevelsComponent : Component
{
    [DataField(required: true)]
    public int DefaultMapLevel = 0;

    [DataField(required: true)]
    public Dictionary<int, CP14ZLevelEntry> Levels = new();

    public bool Initialized = false;

    public Dictionary<MapId, int> LevelEntities = new();
}

[DataRecord, Serializable]
public sealed class CP14ZLevelEntry
{
    public ResPath? Path { get; set; } = null;
}
