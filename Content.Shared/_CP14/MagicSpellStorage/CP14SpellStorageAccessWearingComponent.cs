namespace Content.Shared._CP14.MagicSpellStorage;

/// <summary>
/// Denotes that this item's spells can be accessed while wearing it in your body
/// </summary>
[RegisterComponent, Access(typeof(CP14SpellStorageSystem))]
public sealed partial class CP14SpellStorageAccessWearingComponent : Component
{
}
