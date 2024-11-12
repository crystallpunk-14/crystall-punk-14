namespace Content.Shared._CP14.Shuttle;

/// <summary>
///
/// </summary>
[RegisterComponent, Access(typeof(CP14WatershipSystem))]
public sealed partial class CP14WaterShipPaddleComponent : Component
{
    [DataField]
    public float Power = 5f;

    [DataField]
    public Angle ImpulseAngle = Angle.FromDegrees(90f);
}
