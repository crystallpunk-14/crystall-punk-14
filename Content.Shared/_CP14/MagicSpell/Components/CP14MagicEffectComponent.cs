using Content.Shared._CP14.MagicRitual.Prototypes;
using Content.Shared._CP14.MagicSpell.Spells;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.MagicSpell.Components;

/// <summary>
/// Restricts the use of this action, by spending mana or user requirements.
/// </summary>
[RegisterComponent, Access(typeof(CP14SharedMagicSystem))]
public sealed partial class CP14MagicEffectComponent : Component
{
    [DataField]
    public FixedPoint2 ManaCost = 0f;

    [DataField]
    public ProtoId<CP14MagicTypePrototype>? MagicType = null;

    /// <summary>
    /// Can the cost of casting this magic effect be changed from clothing or other sources?
    /// </summary>
    [DataField]
    public bool CanModifyManacost = true;

    [DataField]
    public bool Safe = false;

    /// <summary>
    /// Effects that will trigger at the beginning of the cast, before mana is spent. Should have no gameplay importance, just special effects, popups and sounds.
    /// </summary>
    [DataField]
    public List<CP14SpellEffect> TelegraphyEffects = new();

    [DataField]
    public List<CP14SpellEffect> Effects = new();
}
