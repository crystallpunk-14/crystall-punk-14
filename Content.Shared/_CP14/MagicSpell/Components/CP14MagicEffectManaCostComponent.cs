using Content.Shared._CP14.MagicSpellStorage;
using Content.Shared.FixedPoint;

namespace Content.Shared._CP14.MagicSpell.Components;

/// <summary>
/// Restricts the use of this action, by spending mana or user requirements.
/// </summary>
[RegisterComponent, Access(typeof(CP14SharedMagicSystem), typeof(CP14SpellStorageSystem))]
public sealed partial class CP14MagicEffectManaCostComponent : Component
{
    [DataField]
    public FixedPoint2 ManaCost = 0f;

    /// <summary>
    /// Can the cost of casting this magic effect be changed from clothing or other sources?
    /// </summary>
    [DataField]
    public bool CanModifyManacost = true;
}
