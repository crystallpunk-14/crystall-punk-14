namespace Content.Shared._CP14.DemiplaneTraveling;

/// <summary>
///
/// </summary>
[RegisterComponent]
public sealed partial class CP14StationDemiplaneMapComponent : Component
{
    [DataField]
    public HashSet<CP14DemiplaneMapNode> Nodes = new();
}
