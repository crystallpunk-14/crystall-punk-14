namespace Content.Shared._CP14.Demiplan.Components;

/// <summary>
/// An entity that is the link between the demiplane and the real world. Depending on whether it is in the real world or in the demiplane
/// </summary>
[RegisterComponent]
public sealed partial class CP14DemiplanRiftComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    [DataField]
    public Entity<CP14DemiplanComponent>? Demiplan;

    /// <summary>
    /// Checks if the map on which this rift is initialized is a demiplane to automatically bind to it. QoL thing.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    [DataField]
    public bool TryAutoLinkToMap = true;

    /// <summary>
    /// will this rift become one of the random entry or exit points of the demiplane
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    [DataField]
    public bool ActiveTeleport = true;

    [DataField]
    public bool DeleteAfterDisconnect = true;
}
