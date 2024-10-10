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

    [DataField]
    public EntityUid TradepostMap;

    [DataField]
    public ResPath ShuttlePath = new("/Maps/_CP14/Ships/baloon.yml");

    [DataField]
    public TimeSpan NextTravelTime = TimeSpan.Zero;

    [DataField]
    public TimeSpan TravelPeriod = TimeSpan.FromSeconds(5);
}
