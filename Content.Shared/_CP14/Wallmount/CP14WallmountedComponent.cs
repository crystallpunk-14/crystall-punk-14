namespace Content.Shared._CP14.Wallmount;

/// <summary>
/// Stores a list of all entities that are “attached” to this object. Destroying this object will destroy all attached entities
/// </summary>
[RegisterComponent, Access(typeof(CP14WallmountSystem))]
public sealed partial class CP14WallmountedComponent : Component
{
    [DataField]
    public HashSet<EntityUid> Attached = new();
}
