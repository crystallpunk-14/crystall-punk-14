using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.LockKey.Components;

/// <summary>
///
/// </summary>
[RegisterComponent]
public sealed partial class CP14StationKeyDistributionComponent : Component
{
    [DataField]
    public Dictionary<ProtoId<CP14LockGroupPrototype>, int> Keys = new();
}
