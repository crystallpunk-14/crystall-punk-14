using Content.Shared._CP14.MagicRitual.Prototypes;
using Content.Shared._CP14.MagicSpell.Spells;
using Content.Shared._CP14.MagicSpellStorage;
using Content.Shared._CP14.MagicSpellStorage.Components;
using Content.Shared.DoAfter;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.MagicSpell.Components;

/// <summary>
/// Stores the results and appearance of the magic effect
/// </summary>
[RegisterComponent, Access(typeof(CP14SharedMagicSystem), typeof(CP14SpellStorageSystem))]
public sealed partial class CP14MagicEffectComponent : Component
{
    /// <summary>
    /// if this effect was provided by an spellstorage, it will be recorded here automatically.
    /// </summary>
    [DataField]
    public EntityUid? SpellStorage;

    [DataField]
    public ProtoId<CP14MagicTypePrototype>? MagicType = null;

    /// <summary>
    /// Effects that will trigger at the beginning of the cast, before mana is spent. Should have no gameplay importance, just special effects, popups and sounds.
    /// </summary>
    [DataField]
    public List<CP14SpellEffect> TelegraphyEffects = new();

    [DataField]
    public List<CP14SpellEffect> Effects = new();

    [DataField]
    public DoAfterId? ActiveDoAfter;
}
