
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
public sealed class CP14DemiplaneMapUiState(HashSet<CP14DemiplaneMapNode> nodes, HashSet<(string, string)>? edges = null) : BoundUserInterfaceState
{
    public HashSet<CP14DemiplaneMapNode> Nodes = nodes;
    public HashSet<(string, string)> Edges = edges ?? new();
}

[Serializable, NetSerializable]
public sealed class CP14DemiplaneMapNode(string key, int level, Vector2 uiPosition, bool start, ProtoId<CP14DemiplaneLocationPrototype>? location = null, List<ProtoId<CP14DemiplaneModifierPrototype>>? modifiers = null)
{
    public string NodeKey = key;
    public int Level = level;
    public Vector2 UiPosition = uiPosition;
    public bool Start = start;

    public ProtoId<CP14DemiplaneLocationPrototype>? Location = location;
    public List<ProtoId<CP14DemiplaneModifierPrototype>>? Modifiers = modifiers;
}
