namespace Content.Shared._CP14.Demiplan.Components;

/// <summary>
/// Holds a connection to the demiplane. This can be used by other components to move entities between this object and the demiplane
/// </summary>
[RegisterComponent]
public sealed partial class CP14DemiplanExitPointComponent : Component
{
    [DataField]
    public Entity<CP14DemiplanComponent>? Link;
}
