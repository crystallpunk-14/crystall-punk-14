using Robust.Shared.Prototypes;

namespace Content.Server._CP14.Demiplane.Components;

/// <summary>
/// Creates a new map of the next level of the demiplane and connects to it via a portal.
/// </summary>
[RegisterComponent, Access(typeof(CP14DemiplaneSystem))]
public sealed partial class CP14DemiplaneRiftComponent : Component
{
    /// <summary>
    /// Blocks the creation of a new demiplane map, after the first one is created.
    /// </summary>
    [DataField]
    public bool CanCreate = true;

    [DataField]
    public EntProtoId AwaitingProto = "CP14DemiplaneRiftAwaiting";

    [DataField]
    public EntProtoId PortalProto = "CP14DemiplaneRiftPortal";

    [DataField]
    public EntityUid? AwaitingEntity;

    [DataField]
    public EntityUid? ScanningTargetMap;

    [DataField]
    public TimeSpan NextScanTime = TimeSpan.Zero;
}
