namespace Content.Server._CP14.Temperature.Fireplace;

/// <summary>
///
/// </summary>

[RegisterComponent, Access(typeof(CP14FireplaceSystem))]
public sealed partial class CP14FireplaceFuelComponent : Component
{
    [DataField]
    public float Fuel = 10f;
}
