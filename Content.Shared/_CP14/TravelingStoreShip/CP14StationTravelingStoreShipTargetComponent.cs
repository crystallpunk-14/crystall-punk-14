using Content.Shared._CP14.TravelingStoreShip.Prototype;
using Content.Shared.Destructible.Thresholds;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._CP14.TravelingStoreShip;

/// <summary>
/// Add to the station so that traveling store ship starts running on it
/// </summary>
[RegisterComponent]
public sealed partial class CP14StationTravelingStoreShipTargetComponent : Component
{
    [DataField]
    public EntityUid Shuttle;

    [DataField]
    public EntityUid TradePostMap;

    [DataField]
    public bool OnStation;

    [DataField]
    public ResPath ShuttlePath = new("/Maps/_CP14/Ships/cargo_shuttle.yml");

    [DataField]
    public TimeSpan NextTravelTime = TimeSpan.Zero;

    [DataField]
    public TimeSpan StationWaitTime = TimeSpan.FromMinutes(6);

    [DataField]
    public TimeSpan TradePostWaitTime = TimeSpan.FromMinutes(4);

    [DataField]
    public Dictionary<ProtoId<CP14StoreBuyPositionPrototype>, int> CurrentBuyPositions = new(); //Proto, price

    [DataField]
    public MinMax SpecialBuyPositionCount = new(1, 2);

    [DataField]
    public Dictionary<ProtoId<CP14StoreSellPositionPrototype>, int> CurrentSellPositions = new(); //Proto, price

    [DataField]
    public MinMax SpecialSellPositionCount = new(1, 2);
}
