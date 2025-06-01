using Content.Shared._CP14.Trading.Prototypes;
using Content.Shared._CP14.Trading.Systems;
using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Trading.Components;

/// <summary>
/// Reflects the entity's level of reputation, debts, and balance sheet in the “outside” world.
/// Used for personal progression in trading systems
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, Access(typeof(CP14SharedTradingPlatformSystem))]
public sealed partial class CP14TradingReputationComponent : Component
{
    /// <summary>
    /// is both a reputation counter for each faction and an indicator of whether that faction is unlocked for that player.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Dictionary<ProtoId<CP14TradingFactionPrototype>, FixedPoint2> Reputation = new();
}
