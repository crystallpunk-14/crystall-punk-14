namespace Content.Shared._CP14.Demiplan.Components;

/// <summary>
/// allows you to exit the demiplane using doAfter
/// </summary>
[RegisterComponent]
public sealed partial class CP14DemiplanExitComponent : Component
{
    /// <summary>
    /// The number of teleportations this teleporter can make before disappearing. Use the negative number to make infinite.
    /// </summary>
    [DataField]
    public int MaxUse = -1;

    [DataField]
    public float DoAfter = 4f;

    /// <summary>
    /// Completely undresses and removes all items from the character before teleportation
    /// </summary>
    [DataField]
    public bool DidItNude = false;

    [DataField]
    public Entity<CP14DemiplanComponent>? Demiplan;
}
