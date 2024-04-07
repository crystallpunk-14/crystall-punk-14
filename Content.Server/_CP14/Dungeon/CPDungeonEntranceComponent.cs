

using Robust.Shared.Map;

namespace Content.Server._CP14.Dungeon;

[RegisterComponent, Access(typeof(CPDungeonSystem))]
public sealed partial class CPDungeonEntranceComponent : Component
{
    [DataField]
    public MapId? NextLevelMapId = null;
}
