namespace Content.Shared._CP14.MagicSpellStorage.Components;

/// <summary>
/// The ability to access spellcasting is limited by the attuning requirement
/// </summary>
[RegisterComponent, Access(typeof(CP14SpellStorageSystem))]
public sealed partial class CP14SpellStorageRequireAttuneComponent : Component
{
}
