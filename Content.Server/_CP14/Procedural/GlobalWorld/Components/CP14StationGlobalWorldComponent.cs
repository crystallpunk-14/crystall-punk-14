using Content.Shared._CP14.Procedural.Prototypes;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.Procedural.GlobalWorld.Components;

/// <summary>
/// Generates the surrounding procedural world on the game map, surrounding the mapped settlement.
/// </summary>
[RegisterComponent, Access(typeof(CP14GlobalWorldSystem))]
public sealed partial class CP14StationGlobalWorldComponent : Component
{
    [DataField]
    public Dictionary<Vector2i, CP14GlobalWorldNode> Nodes = new();

    [DataField]
    public HashSet<(Vector2i, Vector2i)> Edges = new();

    [DataField]
    public int LocationCount = 5;
}

[Serializable]
public sealed class CP14GlobalWorldNode()
{
    public MapId? MapUid;
    public int Level = 0;

    public ProtoId<CP14ProceduralLocationPrototype>? LocationConfig;
    public List<ProtoId<CP14ProceduralModifierPrototype>> Modifiers = new();
}
