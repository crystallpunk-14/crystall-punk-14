
using System.Numerics;
using Content.Shared._CP14.Demiplane.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._CP14.DemiplaneTraveling;

[Serializable, NetSerializable]
public enum CP14DemiplaneMapUiKey
{
    Key,
}

[Serializable, NetSerializable]
public sealed class CP14DemiplaneMapUiState(Dictionary<Vector2i, CP14DemiplaneMapNode> nodes, HashSet<(Vector2i, Vector2i)>? edges = null) : BoundUserInterfaceState
{
    public Dictionary<Vector2i, CP14DemiplaneMapNode> Nodes = nodes;
    public HashSet<(Vector2i, Vector2i)> Edges = edges ?? new();
}

[Serializable, NetSerializable]
public sealed class CP14DemiplaneMapNode(int level, Vector2 uiPosition, bool start, ProtoId<CP14DemiplaneLocationPrototype>? locationConfig = null, List<ProtoId<CP14DemiplaneModifierPrototype>>? modifiers = null)
{
    public int Level = level;
    public Vector2 UiPosition = uiPosition;
    public bool Start = start;

    /// <summary>
    /// This demiplane is now unexplored by anyone and its coordinates can be retrieved
    /// </summary>
    public bool InFrontierZone = false;
    public bool CoordinatesExtracted = false;

    /// <summary>
    /// Exploration finished!
    /// </summary>
    public bool Finished = start;

    public ProtoId<CP14DemiplaneLocationPrototype>? LocationConfig = locationConfig;
    public List<ProtoId<CP14DemiplaneModifierPrototype>> Modifiers = modifiers ?? new();
}
