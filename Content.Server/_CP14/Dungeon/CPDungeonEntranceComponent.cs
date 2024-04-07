

using Robust.Shared.Map;

namespace Content.Server._CP14.Dungeon;

/// <summary>
///
/// </summary>
[RegisterComponent, Access(typeof(CPDungeonSystem))]
public sealed partial class CPDungeonEntranceComponent : Component
{
    /// <summary>
    ///
    /// </summary>
    [DataField]
    public MapId? NextLevelMapId = null;
}
