namespace Content.Shared._CP14.MagicSpellStorage.Components;

/// <summary>
/// The component allows you to use actions nested in an object not through hotbar, but through direct interaction with the object.
/// </summary>
[RegisterComponent, Access(typeof(CP14SpellStorageSystem))]
public sealed partial class CP14SpellStorageInteractionComponent : Component
{
    /// <summary>
    /// The selected action will automatically attempt to be used when interacting with the item.
    /// </summary>
    [DataField]
    public int SelectedAction;
}
