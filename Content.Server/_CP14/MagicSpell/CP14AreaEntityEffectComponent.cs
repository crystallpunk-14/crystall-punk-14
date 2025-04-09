using Content.Shared._CP14.MagicSpell.Spells;
using Content.Shared.Whitelist;

namespace Content.Server._CP14.MagicSpell;

/// <summary>
/// Component that allows an entity to apply effects to other entities in an area.
/// </summary>
[RegisterComponent]
public sealed partial class CP14AreaEntityEffectComponent : Component
{
    [DataField(required: true)]
    public float Range = 1f;

    /// <summary>
    /// How many entities can be subject to EntityEffect? Leave 0 to remove the restriction.
    /// </summary>
    [DataField]
    public int MaxTargets = 0;

    [DataField(required: true, serverOnly: true)]
    public List<CP14SpellEffect> Effects = new();

    [DataField]
    public EntityWhitelist? Whitelist;
}
