using Content.Shared.CrystallPunk.LockKey;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.CrystallPunk.LockKey;

/// <summary>
/// A component of a lock that stores its keyhole shape, complexity, and current state.
/// </summary>
[RegisterComponent]
public sealed partial class CPLockpickComponent : Component
{
    [DataField]
    public int Health = 3;
}
