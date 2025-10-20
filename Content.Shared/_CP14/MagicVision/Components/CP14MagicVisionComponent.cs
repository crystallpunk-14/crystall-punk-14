using Robust.Shared.GameStates;

namespace Content.Shared._CP14.MagicVision;

/// <summary>
/// Allows to see magic vision trace entities
/// </summary>
[RegisterComponent, NetworkedComponent, Obsolete($"Changed to a Status Effect. Use {nameof(CP14MagicVisionStatusEffectComponent)} instead")]
public sealed partial class CP14MagicVisionComponent : Component
{
}
