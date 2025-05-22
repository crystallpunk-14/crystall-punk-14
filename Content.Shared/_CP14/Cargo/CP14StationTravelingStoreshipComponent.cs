using Content.Shared._CP14.Cargo.Prototype;
using Content.Shared.Destructible.Thresholds;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._CP14.Cargo;

/// <summary>
/// Add to the station so that traveling store ship starts running on it
/// </summary>
[RegisterComponent]
public sealed partial class CP14StationTravelingStoreShipComponent : Component
{
    [DataField]
    public EntityUid? Shuttle;

    [DataField]
    public EntityUid? TradePostMap;

    [DataField]
    public bool OnStation;

    [DataField]
    public ResPath ShuttlePath = new("/Maps/_CP14/Shuttle/cargo_shuttle.yml");

    /// <summary>
    /// Available to random selecting and pusharing
    /// </summary>
    [DataField]
    public HashSet<CP14StoreBuyPositionPrototype> AvailableBuyPosition = new();

    /// <summary>
    /// Fixed prices and positions of the current flight
    /// </summary>
    [DataField]
    public Dictionary<CP14StoreBuyPositionPrototype, int> CurrentBuyPositions = new(); //Proto, price

    [DataField]
    public Dictionary<CP14StoreBuyPositionPrototype, int> CurrentSpecialBuyPositions = new(); //Proto, price

    [DataField]
    public MinMax SpecialBuyPositionCount = new(1, 2);

    [DataField]
    public int Balance = 0;

    /// <summary>
    /// a queue of purchased items. The oldest purchases are taken out one by one to be unloaded onto the ship
    /// </summary>
    [DataField]
    public Queue<EntProtoId> BuyedQueue = new();
}
