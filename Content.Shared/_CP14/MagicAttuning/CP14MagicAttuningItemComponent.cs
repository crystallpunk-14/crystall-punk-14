namespace Content.Shared._CP14.MagicAttuning;

/// <summary>
/// Reflects the fact that this subject can be focused on (Magical attune as a mechanic from DnD.)
/// </summary>
[RegisterComponent, Access(typeof(CP14SharedMagicAttuningSystem))]
public sealed partial class CP14MagicAttuningItemComponent : Component
{
    /// <summary>
    /// how long it takes to focus on that object
    /// </summary>
    [DataField]
    public TimeSpan FocusTime = TimeSpan.FromSeconds(5f);

    public Entity<CP14MagicAttuningMindComponent>? Link = null;
}
