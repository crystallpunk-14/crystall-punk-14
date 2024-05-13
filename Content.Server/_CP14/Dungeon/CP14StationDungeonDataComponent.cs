using Content.Shared._CP14.Dungeon;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.Dungeon;

/// <summary>
///
/// </summary>
[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class CP14StationDungeonDataComponent : Component
{
    [DataField]
    public float StartLootBudget = 15f;

    [DataField]
    public float StartMobBudget = 10f;

    [DataField]
    public List<ProtoId<CP14DungeonLayerPrototype>> AllowedLayers = new ();

    [DataField]
    public int Layers = 3;

    public List<MapId> MapsByDepth = new();
}
