using Content.Shared._CP14.Trading.Systems;

namespace Content.Shared._CP14.Trading.Components;

/// <summary>
/// Reflects the entity's level of reputation, debts, and balance sheet in the “outside” world.
/// Used for personal progression in trading systems
/// </summary>
[RegisterComponent, Access(typeof(CP14SharedTradingPlatformSystem))]
public sealed partial class CP14TradingReputationComponent : Component
{
}
