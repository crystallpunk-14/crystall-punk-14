namespace Content.Shared._CP14.Demiplan.Components;

/// <summary>
/// Stores the entry point to the demiplane. One of the entry points contains entities that have entered the demiplane.
/// </summary>
[RegisterComponent]
public sealed partial class CP14DemiplanEntryPointComponent : Component
{
    [DataField]
    public Entity<CP14DemiplanComponent>? Link;
}
