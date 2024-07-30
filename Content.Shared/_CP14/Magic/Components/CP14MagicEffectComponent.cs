using Content.Shared.FixedPoint;

namespace Content.Shared._CP14.Magic.Components;

/// <summary>
/// Restricts the use of this action, by spending mana or user requirements.
/// </summary>
[RegisterComponent, Access(typeof(CP14SharedMagicSystem))]
public sealed partial class CP14MagicEffectComponent : Component
{
    [DataField]
    public FixedPoint2 ManaCost = 0f;

    [DataField]
    public bool Safe = false;
}
