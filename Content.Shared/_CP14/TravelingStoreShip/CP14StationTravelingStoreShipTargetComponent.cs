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
    public TimeSpan StationWaitTime = TimeSpan.FromMinutes(1);

    [DataField]
    public TimeSpan TradePostWaitTime = TimeSpan.FromMinutes(1);

    [DataField]
    public Dictionary<CP14StoreBuyPositionPrototype, int> CurrentBuyPositions = new(); //Proto, price

    [DataField]
    public MinMax SpecialBuyPositionCount = new(1, 2);

    [DataField]
    public Dictionary<CP14StoreSellPositionPrototype, int> CurrentSellPositions = new(); //Proto, price

    [DataField]
    public MinMax SpecialSellPositionCount = new(1, 2);

    /// <summary>
    /// a queue of purchased items. The oldest purchases are taken out one by one to be unloaded onto the ship
    /// </summary>
    [DataField]
    public Queue<KeyValuePair<CP14StoreBuyPositionPrototype, int>> BuyedQueue = new();
}
