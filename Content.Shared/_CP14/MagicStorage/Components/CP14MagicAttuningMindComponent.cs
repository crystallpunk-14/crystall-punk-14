namespace Content.Shared._CP14.MagicStorage.Components;

/// <summary>
/// A mind that can focus on objects
/// </summary>
[RegisterComponent, Access(typeof(CP14SharedMagicStorageSystem))]
public sealed partial class CP14MagicAttuningMindComponent : Component
{
    [DataField]
    public int MaxFocus = 3;
    /// <summary>
    /// The entities that this being is focused on
    /// </summary>
    public List<Entity<CP14MagicAttuningItemComponent>> FocusingEntities = new();
}
