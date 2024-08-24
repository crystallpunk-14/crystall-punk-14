using Content.Server._CP14.StationDungeonMap.EntitySystems;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.StationDungeonMap.Components;

/// <summary>
///
/// </summary>
[RegisterComponent, Access(typeof(CP14StationZLevelsSystem))]
public sealed partial class CP14ZLevelAutoPortalComponent : Component
{
    [DataField(required: true)]
    public int ZLevelOffset = 0;

    [DataField(required: true)]
    public EntProtoId OtherSideProto = default!;
}
