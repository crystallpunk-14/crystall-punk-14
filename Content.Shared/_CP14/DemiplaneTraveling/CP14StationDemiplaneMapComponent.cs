using Content.Shared.Destructible.Thresholds;

namespace Content.Shared._CP14.DemiplaneTraveling;

/// <summary>
///
/// </summary>
[RegisterComponent]
public sealed partial class CP14StationDemiplaneMapComponent : Component
{
    [DataField]
    public Dictionary<Vector2i, CP14DemiplaneMapNode> Nodes = new();

    [DataField]
    public HashSet<(Vector2i, Vector2i)> Edges = new();

    /// <summary>
    /// Count of special rooms that can be generated in the demiplane map.
    /// </summary>
    [DataField]
    public MinMax Specials = new(30, 30);
}
