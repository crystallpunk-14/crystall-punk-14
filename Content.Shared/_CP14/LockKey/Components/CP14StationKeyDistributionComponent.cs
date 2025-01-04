using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.LockKey.Components;

/// <summary>
///
/// </summary>
[RegisterComponent]
public sealed partial class CP14StationKeyDistributionComponent : Component
{
    [DataField]
    public List<ProtoId<CP14LockTypePrototype>> Keys = new();
}
