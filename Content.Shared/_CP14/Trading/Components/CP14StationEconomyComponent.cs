using Content.Shared._CP14.Trading.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Trading.Components;

/// <summary>
///
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CP14StationEconomyComponent : Component
{
    [DataField, AutoNetworkedField]
    public Dictionary<ProtoId<CP14TradingPositionPrototype>, int> Pricing = new();
}
