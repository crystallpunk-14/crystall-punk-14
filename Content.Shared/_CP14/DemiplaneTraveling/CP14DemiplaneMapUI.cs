
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
public sealed class CP14DemiplaneMapUiState(HashSet<CP14DemiplaneMapNode> nodes) : BoundUserInterfaceState
{
    public HashSet<CP14DemiplaneMapNode> Nodes = nodes;
}

[Serializable, NetSerializable]
public sealed class CP14DemiplaneMapNode(string key, Vector2 uiPosition, bool start, ProtoId<CP14DemiplaneLocationPrototype> location, List<ProtoId<CP14DemiplaneModifierPrototype>> modifiers, HashSet<string>? childrens = null)
{
    public string NodeKey = key;
    public Vector2 UiPosition = uiPosition;
    public bool Start = start;

    public HashSet<string> Childrens = childrens ?? new HashSet<string>();

    public ProtoId<CP14DemiplaneLocationPrototype> Location = location;
    public List<ProtoId<CP14DemiplaneModifierPrototype>> Modifiers = modifiers;
}
