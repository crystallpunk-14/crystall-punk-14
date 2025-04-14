namespace Content.Server._CP14.RoundLeave;

/// <summary>
/// Allows a player to leave the round if they exit into a ghost while inside
/// </summary>
[RegisterComponent]
public sealed partial class CP14RoundLeaveComponent : Component
{
}


/// <summary>
/// Marker on entities. If a player goes ghost with this component, he leaves the round and regains the role.
/// </summary>
[RegisterComponent]
public sealed partial class CP14RoundLeavingComponent : Component
{
    [DataField]
    public HashSet<EntityUid> Leaver = new();
}
