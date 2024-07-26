using Content.Shared.FixedPoint;

namespace Content.Shared._CP14.Magic;

/// <summary>
///
/// </summary>
[RegisterComponent, /*Access(typeof())*/]
public sealed partial class CP14MagicEffectComponent : Component
{
    [DataField]
    public FixedPoint2 ManaCost = 0f;
}
