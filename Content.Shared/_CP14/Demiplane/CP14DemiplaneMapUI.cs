using System.Numerics;
using Content.Shared._CP14.Procedural.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._CP14.Demiplane;

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
public sealed class CP14DemiplaneMapNode(Vector2 uiPosition, ProtoId<CP14ProceduralLocationPrototype>? locationConfig = null, List<ProtoId<CP14ProceduralModifierPrototype>>? modifiers = null)
{
    public bool Start = false;
    public int AdditionalLevel = 0;
    public Vector2 UiPosition = uiPosition;

    public bool Opened = false;

    public ProtoId<CP14ProceduralLocationPrototype>? LocationConfig = locationConfig;
    public List<ProtoId<CP14ProceduralModifierPrototype>> Modifiers = modifiers ?? [];
}
