using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.LockKey.Components;

/// <summary>
/// a key component that can be used to unlock and lock locks from CPLockComponent
/// </summary>
[RegisterComponent, AutoGenerateComponentState(fieldDeltas: true), NetworkedComponent]
public sealed partial class CP14KeyComponent : Component
{
    [DataField, AutoNetworkedField]
    public List<int>? LockShape = null;

    /// <summary>
    /// If not null, automatically generates a key for the specified category on initialization. This ensures that the lock will be opened with a key of the same category.
    /// </summary>
    [DataField]
    public ProtoId<CP14LockTypePrototype>? AutoGenerateShape = null;
}
