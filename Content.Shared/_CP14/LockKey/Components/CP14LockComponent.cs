using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.LockKey.Components;

/// <summary>
/// A component of a lock that stores its keyhole shape, complexity, and current state.
/// </summary>
[RegisterComponent]
public sealed partial class CP14LockComponent : Component
{
    [DataField]
    public List<int>? LockShape = null;

    [DataField]
    public float LockPickDamageChance = 0.2f;

    /// <summary>
    /// On which element of the shape sequence the lock is now located. It's necessary for the mechanics of breaking and entering.
    /// </summary>
    [DataField]
    public int LockpickStatus = 0;

    /// <summary>
    /// after a lock is broken into, it leaves a description on it that it's been tampered with.
    /// </summary>
    [DataField]
    public bool LockpickeddFailMarkup = false;

    /// <summary>
    /// If not null, automatically generates a lock for the specified category on initialization. This ensures that the lock will be opened with a key of the same category.
    /// </summary>
    [DataField]
    public ProtoId<CP14LockCategoryPrototype>? AutoGenerateShape = null;
}
