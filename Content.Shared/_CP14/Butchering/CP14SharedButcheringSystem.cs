using Robust.Shared.GameStates;

namespace Content.Shared._CP14.Butchering;

/// <summary>
/// Shared stub for CP14 butchering. Kept to mirror the modular pattern used by CP14 Cooking:
/// - Shared system exists for access control and potential shared logic later.
/// - Server system contains the gameplay logic.
/// </summary>
public sealed class CP14SharedButcheringSystem : EntitySystem
{
}
