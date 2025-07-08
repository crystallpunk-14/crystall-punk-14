namespace Content.Server._CP14.MagicSpellStorage.Components;

/// <summary>
/// Denotes that this item's spells can be accessed while wearing it on your body
/// </summary>
[RegisterComponent, Access(typeof(CP14SpellStorageSystem))]
public sealed partial class CP14SpellStorageAccessWearingComponent : Component
{
    [DataField]
    public bool Wearing;
}
