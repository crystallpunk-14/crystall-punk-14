
using System.Numerics;
using Robust.Shared.Serialization;

namespace Content.Shared._CP14.DemiplaneTraveling;

[Serializable, NetSerializable]
public enum CP14DemiplaneMapUiKey
{
    Key,
}

[Serializable, NetSerializable]
public sealed class CP14DemiplaneMapUiState(HashSet<CP14DemiplaneMapNode> nodes) : BoundUserInterfaceState
{
    public HashSet<CP14DemiplaneMapNode> Nodes = nodes;
}

[Serializable, NetSerializable]
public sealed class CP14DemiplaneMapNode(Vector2 position)
{
    public Vector2 Position = position;
}
