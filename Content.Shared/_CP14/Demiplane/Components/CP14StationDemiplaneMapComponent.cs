namespace Content.Shared._CP14.Demiplane.Components;

/// <summary>
/// A station component that stores information about the current map of demiplanes, their research status and relationships.
/// </summary>
[RegisterComponent]
public sealed partial class CP14StationDemiplaneMapComponent : Component
{
    [DataField]
    public Dictionary<Vector2i, CP14DemiplaneMapNode> Nodes = new();

    [DataField]
    public HashSet<(Vector2i, Vector2i)> Edges = new();

    [DataField]
    public int TotalCount = 15;
}
