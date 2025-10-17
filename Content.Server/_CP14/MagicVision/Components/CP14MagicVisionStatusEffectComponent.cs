using Content.Shared.Eye;
using Robust.Shared.GameStates;

namespace Content.Server._CP14.MagicVision;

/// <summary>
/// Allows to see magic vision trace entities
/// Use only in conjunction with <see cref="StatusEffectComponent"/>, on the status effect entity.
/// </summary>
[RegisterComponent]
public sealed partial class CP14MagicVisionStatusEffectComponent : Component
{
    /// <summary>
    /// VisionMask to see Magic Vision layer
    /// </summary>
    [DataField]
    public const VisibilityFlags VisibilityMask = VisibilityFlags.CP14MagicVision;
}
