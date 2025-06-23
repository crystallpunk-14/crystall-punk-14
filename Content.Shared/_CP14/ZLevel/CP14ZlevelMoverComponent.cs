using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.ZLevel;

/// <summary>
/// component that allows you to quickly move between Z levels
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CP14ZLevelMoverComponent : Component
{
    [DataField]
    public EntProtoId UpActionProto = "CP14ActionZLevelUp";

    [DataField, AutoNetworkedField]
    public EntityUid? CP14ZLevelUpActionEntity;

    [DataField]
    public EntProtoId DownActionProto = "CP14ActionZLevelDown";

    [DataField, AutoNetworkedField]
    public EntityUid? CP14ZLevelDownActionEntity;
}
