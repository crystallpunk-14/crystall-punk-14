
using Content.Shared.CrystallPunk.LockKey;
using Robust.Shared.Prototypes;

namespace Content.Server.CrystallPunk.LockKey;


/// <summary>
/// A component that allows you to put a lock on an entity. Additionally, it allows generating locks of a certain type during initialization.
/// </summary>
[RegisterComponent, Access(typeof(CPLockKeySystem))]
public sealed partial class CPLockHolderComponent : Component
{
    [DataField]
    public EntityUid? LockEntity = null;

    /// <summary>
    /// If not null, automatically generates a lock for the specified category on initialization. This ensures that the lock will be opened with a key of the same category.
    /// </summary>
    [DataField]
    public ProtoId<CPLockCategoryPrototype>? AutoGenerateLock = null;
}
