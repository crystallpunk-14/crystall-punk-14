using Content.Shared._CP14.Trading.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._CP14.Trading.Components;

/// <summary>
/// The server calculates all prices for all product items, saves them in this component at the station,
/// and synchronizes the data with the client for the entire round.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CP14StationEconomyComponent : Component
{
    [DataField, AutoNetworkedField]
    public Dictionary<ProtoId<CP14TradingPositionPrototype>, int> Pricing = new();

    [DataField, AutoNetworkedField]
    public Dictionary<ProtoId<CP14TradingRequestPrototype>, int> RequestPricing = new();

    [DataField, AutoNetworkedField]
    public Dictionary<ProtoId<CP14TradingFactionPrototype>, HashSet<ProtoId<CP14TradingRequestPrototype>> > ActiveRequests = new();

    [DataField]
    public int MaxRequestCount = 5;
}
