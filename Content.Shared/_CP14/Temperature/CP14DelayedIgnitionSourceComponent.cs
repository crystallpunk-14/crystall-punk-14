namespace Content.Shared._CP14.Temperature;

/// <summary>
/// Allows you to fire entities through interacting with them after a delay.
/// </summary>
[RegisterComponent, Access(typeof(CP14SharedFireSpreadSystem))]
public sealed partial class CP14DelayedIgnitionSourceComponent : Component
{
    [DataField]
    public TimeSpan Delay = TimeSpan.FromSeconds(2f);
}
