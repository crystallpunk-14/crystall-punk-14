using Content.Shared._CP14.MagicSpell.Spells;
using Content.Shared.FixedPoint;

namespace Content.Shared._CP14.MagicSpell.Components;

/// <summary>
/// Slows the caster while using this spell
/// </summary>
[RegisterComponent, Access(typeof(CP14SharedMagicSystem))]
public sealed partial class CP14MagicEffectCastSlowdownComponent : Component
{
    [DataField]
    public float SpeedMultiplier = 1f;
}
