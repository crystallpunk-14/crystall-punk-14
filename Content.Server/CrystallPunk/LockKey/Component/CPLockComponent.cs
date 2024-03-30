
namespace Content.Server.CrystallPunk.LockKey;

/// <summary>
/// A component of a lock that stores its keyhole shape, complexity, and current state.
/// </summary>
[RegisterComponent, Access(typeof(CPLockKeySystem))]
public sealed partial class CPLockComponent : Component
{
    [DataField]
    public List<int>? LockShape = null;

    [DataField]
    public float LockPickBreakChance = 0.3f;
}
