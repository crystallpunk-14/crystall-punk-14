using Robust.Shared.Utility;

namespace Content.Server._CP14.TravelingStoreShip;

/// <summary>
/// Add to the station so that traveling storeship starts running on it
/// </summary>
[RegisterComponent, Access(typeof(CP14TravelingStoreShipSystem))]
public sealed partial class CP14StationTravelingStoreshipTargetComponent : Component
{
    [DataField]
    public EntityUid Shuttle;

    [DataField] public ResPath ShuttlePath = new("/Maps/Shuttles/arrivals.yml");
}
