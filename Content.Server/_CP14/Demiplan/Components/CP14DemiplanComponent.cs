namespace Content.Server._CP14.Demiplan.Components;

/// <summary>
/// Designates this entity as holding a demiplan.
/// </summary>
[RegisterComponent]
public sealed partial class CP14DemiplanComponent : Component
{
    /// <summary>
    /// All entities that are entry points to this demiplane.
    /// </summary>
    [DataField]
    public HashSet<Entity<CP14DemiplanConnectionComponent>> Connections = new();
}
