

using Robust.Shared.Map;

namespace Content.Server._CP14.Dungeon;

/// <summary>
///
/// </summary>
[RegisterComponent, Access(typeof(CP14DungeonSystem))]
public sealed partial class CP14DungeonEntranceComponent : Component
{
    /// <summary>
    ///
    /// </summary>
    [DataField]
    public MapId? NextLevelMapId = null;
}
