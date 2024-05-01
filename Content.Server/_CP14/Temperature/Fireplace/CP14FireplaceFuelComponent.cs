namespace Content.Server._CP14.Temperature.Fireplace;

/// <summary>
/// Allows this object to be used as fuel for a fireplace
/// </summary>

[RegisterComponent, Access(typeof(CP14FireplaceSystem))]
public sealed partial class CP14FireplaceFuelComponent : Component
{
    /// <summary>
    /// How much fuel will be added in fireplace
    /// </summary>
    [DataField]
    public float Fuel = 10f;
}
