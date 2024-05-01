namespace Content.Server._CP14.Temperature.Fireplace;

/// <summary>
///
/// </summary>

[RegisterComponent, Access(typeof(CP14FireplaceSystem))]
public sealed partial class CP14FireplaceComponent : Component
{
    public bool CanIgnite => FuelDrainingPerUpdate >= CurrentFuel;

    [DataField]
    public float MaxFuelLimit = 100f;

    [DataField]
    public float FireFadeDelta = 0.2f;

    [DataField]
    public float CurrentFuel;

    [DataField]
    public float FuelDrainingPerUpdate = 1f;

    [DataField]
    public bool CanInsertByHand = true;

    [DataField]
    public bool CanInsertByThrow = false;

    [DataField]
    public bool CanInsertByCollide = false;

    [DataField]
    public TimeSpan UpdateFrequency = TimeSpan.FromSeconds(2f);

    [DataField]
    public TimeSpan NextUpdateTime = TimeSpan.Zero;
}
