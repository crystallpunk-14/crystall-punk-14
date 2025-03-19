using Content.Shared._CP14.Cargo.Prototype;
using Content.Shared.Destructible.Thresholds;
using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Cargo;

/// <summary>
/// Add to the station so ...
/// </summary>
[NetworkedComponent, RegisterComponent, AutoGenerateComponentPause, AutoGenerateComponentState]
public sealed partial class CP14TradingPortalComponent : Component
{
    [DataField(required: true), AutoNetworkedField]
    public ProtoId<CP14StoreFactionPrototype> Faction = default;

    [DataField]
    public EntityCoordinates? TradingPosition = null;

    [DataField, AutoNetworkedField]
    public TimeSpan Delay = TimeSpan.FromSeconds(3f);

    [DataField, AutoNetworkedField]
    [AutoPausedField]
    public TimeSpan ProcessFinishTime = TimeSpan.Zero;

    /// <summary>
    /// Available to random selecting and pusharing
    /// </summary>
    [DataField]
    public HashSet<CP14StoreBuyPositionPrototype> AvailableBuyPosition = new();

    /// <summary>
    /// Available to random selecting and pusharing
    /// </summary>
    [DataField]
    public HashSet<CP14StoreSellPositionPrototype> AvailableSellPosition = new();

    /// <summary>
    /// Fixed prices and positions of the current flight
    /// </summary>
    [DataField]
    public Dictionary<CP14StoreBuyPositionPrototype, int> CurrentBuyPositions = new(); //Proto, price

    [DataField]
    public Dictionary<CP14StoreBuyPositionPrototype, int> CurrentSpecialBuyPositions = new(); //Proto, price

    [DataField]
    public int SpecialBuyPositionCount = 2;

    [DataField]
    public Dictionary<CP14StoreSellPositionPrototype, int> CurrentSellPositions = new(); //Proto, price

    [DataField]
    public Dictionary<CP14StoreSellPositionPrototype, int> CurrentSpecialSellPositions = new(); //Proto, price

    [DataField]
    public int SpecialSellPositionCount = 2;

    [DataField]
    public int Balance = 0;
}
