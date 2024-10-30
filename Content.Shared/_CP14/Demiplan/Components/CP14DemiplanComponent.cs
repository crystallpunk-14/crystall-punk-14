namespace Content.Shared._CP14.Demiplan.Components;

/// <summary>
/// Designates this entity as holding a demiplan.
/// </summary>
[RegisterComponent]
public sealed partial class CP14DemiplanComponent : Component
{
    /// <summary>
    /// All entities in the real world that are connected to this demiplane
    /// </summary>
    [DataField]
    public HashSet<Entity<CP14DemiplanConnectionComponent>> Connections = new();

    /// <summary>
    /// All entities in the demiplane in which the objects entered in the demiplane appear
    /// </summary>
    [DataField]
    public HashSet<Entity<CP14DemiplanEntryPointComponent>> EntryPoints = new();
}
