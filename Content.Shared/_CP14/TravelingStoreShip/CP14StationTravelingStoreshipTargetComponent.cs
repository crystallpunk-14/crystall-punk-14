using Content.Shared._CP14.TravelingStoreShip.Prototype;
using Content.Shared.Destructible.Thresholds;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._CP14.TravelingStoreShip;

/// <summary>
/// Add to the station so that traveling storeship starts running on it
/// </summary>
[RegisterComponent]
public sealed partial class CP14StationTravelingStoreshipTargetComponent : Component
{
    [DataField]
    public EntityUid Shuttle;

    [DataField]
    public EntityUid TradepostMap;

    [DataField]
    public bool OnStation = false;

    [DataField]
    public ResPath ShuttlePath = new("/Maps/_CP14/Ships/baloon.yml");

    [DataField]
    public TimeSpan NextTravelTime = TimeSpan.Zero;

    [DataField]
    public TimeSpan StationWaitTime = TimeSpan.FromSeconds(30);

    [DataField]
    public TimeSpan TradepostWaitTime = TimeSpan.FromSeconds(30);

    //Buy
    [DataField(required: true)]
    public List<ProtoId<CP14StoreBuyPositionPrototype>> StaticBuyPositions = new();

    [DataField]
    public List<ProtoId<CP14StoreBuyPositionPrototype>> DynamicBuyPositions = new();

    [DataField]
    public Dictionary<ProtoId<CP14StoreBuyPositionPrototype>, (int, bool)> CurrentBuyPositions = new(); //Proto, (price, special)

    [DataField]
    public MinMax SpecialBuyPositionCount = new MinMax(1, 2);

    //Sell
    [DataField(required: true)]
    public List<ProtoId<CP14StoreSellPositionPrototype>> StaticSellPositions = new();

    [DataField]
    public List<ProtoId<CP14StoreSellPositionPrototype>> DynamicSellPositions = new();

    [DataField]
    public Dictionary<ProtoId<CP14StoreSellPositionPrototype>, (int, bool)> CurrentSellPositions = new(); //Proto, (price, special)

    [DataField]
    public MinMax SpecialSellPositionCount = new MinMax(1, 2);
}
