using Content.Shared.CrystallPunk.LockKey;
using Robust.Shared.Prototypes;

namespace Content.Shared.CrystallPunk.LockKey;

/// <summary>
///
/// </summary>
[RegisterComponent]
public sealed partial class CPLockSlotComponent : Component
{
    [DataField(required: true)]
    public string LockSlotId = string.Empty;
}
