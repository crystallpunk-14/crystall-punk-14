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
    public ResPath ShuttlePath = new("/Maps/_CP14/Ships/balloon.yml");

    [DataField]
    public TimeSpan NextTravelTime = TimeSpan.Zero;

    [DataField]
    public TimeSpan StationWaitTime = TimeSpan.FromMinutes(1);

    [DataField]
    public TimeSpan TradePostWaitTime = TimeSpan.FromMinutes(1);

    //Buy
    [DataField]
    public List<ProtoId<CP14StoreBuyPositionPrototype>> StaticBuyPositions = new();

    [DataField]
    public List<ProtoId<CP14StoreBuyPositionPrototype>> DynamicBuyPositions = new();

    [DataField]
    public Dictionary<ProtoId<CP14StoreBuyPositionPrototype>, (int, bool)> CurrentBuyPositions = new(); //Proto, (price, special)

    [DataField]
    public MinMax SpecialBuyPositionCount = new(1, 2);

    //Sell
    [DataField]
    public List<ProtoId<CP14StoreSellPositionPrototype>> StaticSellPositions = new();

    [DataField]
    public List<ProtoId<CP14StoreSellPositionPrototype>> DynamicSellPositions = new();

    [DataField]
    public Dictionary<ProtoId<CP14StoreSellPositionPrototype>, (int, bool)> CurrentSellPositions = new(); //Proto, (price, special)

    [DataField]
    public MinMax SpecialSellPositionCount = new(1, 2);
}
