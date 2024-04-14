namespace Content.Server._CP14.Temperature;

/// <summary>
/// A component that allows fire to spread to nearby objects. The basic mechanics of a spreading fire
/// </summary>

[RegisterComponent, Access(typeof(CPFireSpreadSystem))]
public sealed partial class CPFireSpreadComponent : Component
{
    /// <summary>
    /// radius of ignition of neighboring objects
    /// </summary>
    [DataField]
    public float Radius = 1f;

    /// <summary>
    /// chance of spreading to neighboring properties
    /// </summary>
    [DataField]
    public float Prob = 0.5f;

    /// <summary>
    /// how often objects will try to set the neighbors on fire. In Seconds
    /// </summary>
    [DataField]
    public float SpreadCooldownMin = 3f;

    /// <summary>
    /// how often objects will try to set the neighbors on fire. In Seconds
    /// </summary>
    [DataField]
    public float SpreadCooldownMax = 7f;

    /// <summary>
    /// the time of the next fire spread
    /// </summary>
    [DataField]
    public TimeSpan NextSpreadTime { get; set; } = TimeSpan.Zero;
}
