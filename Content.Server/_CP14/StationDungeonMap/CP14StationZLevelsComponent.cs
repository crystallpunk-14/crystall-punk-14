using Robust.Shared.Map;
using Robust.Shared.Utility;

namespace Content.Server._CP14.StationDungeonMap;

/// <summary>
///
/// </summary>
[RegisterComponent, Access(typeof(CP14StationZLevelsSystem))]
public sealed partial class CP14StationZLevelsComponent : Component
{
    [DataField(required: true)]
    public int DefaultMapLevel = 0;

    [DataField]
    public Dictionary<int, CP14ZLevelEntry> Levels = new();
}

[DataRecord, Serializable]
public sealed class CP14ZLevelEntry
{
    /// <summary>
    ///
    /// </summary>
    public ResPath? Path { get; set; } = null;

    /// <summary>
    ///
    /// </summary>
    public MapId? GeneratedMap { get; set; } = null;
}
