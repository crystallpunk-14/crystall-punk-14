using Content.Shared.Eye;
using Robust.Shared.GameStates;

namespace Content.Shared._CP14.MagicVision;

/// <summary>
/// Allows to see magic vision trace entities
/// Use only in conjunction with <see cref="StatusEffectComponent"/>, on the status effect entity.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class CP14MagicVisionStatusEffectComponent : Component
{
    /// <summary>
    /// Enables or disables Magic Vision Overlay
    /// </summary>
    [DataField]
    public bool Overlay = true;

    /// <summary>
    /// VisionMask to see Magic Vision layer
    /// </summary>
    [DataField]
    public const VisibilityFlags VisibilityMask = VisibilityFlags.CP14MagicVision;
}
