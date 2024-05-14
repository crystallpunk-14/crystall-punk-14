using Content.Server._CP14.StationLoadmap;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.DungeonPortal;

/// <summary>
///
/// </summary>
[RegisterComponent, Access(typeof(CP14AutoDungeonPortalSystem))]
public sealed partial class CP14AutoDungeonPortalComponent : Component
{
    [DataField(required: true)]
    public EntProtoId OtherSidePortal;
}
