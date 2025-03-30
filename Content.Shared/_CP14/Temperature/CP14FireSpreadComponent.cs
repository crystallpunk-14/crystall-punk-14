namespace Content.Shared._CP14.Temperature;

/// <summary>
/// A component that allows fire to spread to nearby objects. The basic mechanics of a spreading fire
/// </summary>

[RegisterComponent, Access(typeof(CP14SharedFireSpreadSystem))]
public sealed partial class CP14FireSpreadComponent : Component
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
    public float Prob = 0.1f;

    /// <summary>
    /// chance of tile spreading to neighboring properties
    /// </summary>
    [DataField]
    public float ProbTile = 0.1f;

    /// <summary>
    /// how often objects will try to set the neighbors on fire. In Seconds
    /// </summary>
    [DataField]
    public float SpreadCooldownMin = 10f;

    /// <summary>
    /// how often objects will try to set the neighbors on fire. In Seconds
    /// </summary>
    [DataField]
    public float SpreadCooldownMax = 20f;

    /// <summary>
    /// the time of the next fire spread
    /// </summary>
    [DataField]
    public TimeSpan NextSpreadTime { get; set; } = TimeSpan.Zero;
}
