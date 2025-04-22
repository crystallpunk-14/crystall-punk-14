namespace Content.Shared._CP14.LockKey.Components;

/// <summary>
/// Allows, when interacting with keys, to mill different teeth, changing the shape of the key
/// </summary>
[RegisterComponent]
public sealed partial class CP14KeyFileComponent : Component
{
    [DataField]
    public TimeSpan Delay = TimeSpan.FromSeconds(1);
}
