using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.LockKey.Components;

/// <summary>
/// A component of a lock that stores its keyhole shape, complexity, and current state.
/// </summary>
[RegisterComponent, AutoGenerateComponentState(fieldDeltas: true), NetworkedComponent]
public sealed partial class CP14LockComponent : Component
{
    [DataField, AutoNetworkedField]
    public List<int>? LockShape = null;

    [DataField, AutoNetworkedField]
    public float LockPickDamageChance = 0.2f;

    /// <summary>
    /// On which element of the shape sequence the lock is now located. It's necessary for the mechanics of breaking and entering.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int LockPickStatus = 0;

    /// <summary>
    /// If not null, automatically generates a lock for the specified category on initialization. This ensures that the lock will be opened with a key of the same category.
    /// </summary>
    [DataField]
    public ProtoId<CP14LockTypePrototype>? AutoGenerateShape = null;

    /// <summary>
    /// If not null, the lock will automatically generate a random shape on initialization with selected numbers of elements. Useful for random dungeons doors or chests for example.
    /// </summary>
    [DataField]
    public int? AutoGenerateRandomShape = null;

    /// <summary>
    /// This component is used for two types of items: Entities themselves that are locked (doors, chests),
    /// and a portable lock item that can be built into other entities. This variable determines whether
    /// using this entity on another entity can overwrite the lock properties of the target entity.
    /// </summary>
    [DataField]
    public bool CanEmbedded = false;
}
