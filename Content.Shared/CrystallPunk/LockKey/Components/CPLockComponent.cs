using Content.Shared.CrystallPunk.LockKey;
using Robust.Shared.Prototypes;

namespace Content.Shared.CrystallPunk.LockKey;

/// <summary>
/// A component of a lock that stores its keyhole shape, complexity, and current state.
/// </summary>
[RegisterComponent, Access(typeof(SharedCPLockKeySystem))]
public sealed partial class CPLockComponent : Component
{
    [DataField]
    public List<int>? LockShape = null;

    [DataField]
    public float LockPickBreakChance = 0.3f;

    [DataField]
    public bool Locked = true;

    /// <summary>
    /// Shitcode moment. locked locks can't be hung on doors. Locked locks must be hung by roundstart. This is a one-time exception for roundstart locks.
    /// </summary>
    [DataField]
    public bool RoundstartLockInit = false;

    /// <summary>
    /// If not null, automatically generates a lock for the specified category on initialization. This ensures that the lock will be opened with a key of the same category.
    /// </summary>
    [DataField]
    public ProtoId<CPLockCategoryPrototype>? AutoGenerateLock = null;
}
