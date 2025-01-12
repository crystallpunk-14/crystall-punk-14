namespace Content.Shared._CP14.MagicSpellStorage.Components;

/// <summary>
/// Denotes that this item's spells can be accessed while holding it in your hand
/// </summary>
[RegisterComponent, Access(typeof(CP14SpellStorageSystem))]
public sealed partial class CP14SpellStorageAccessHoldingComponent : Component
{
}
