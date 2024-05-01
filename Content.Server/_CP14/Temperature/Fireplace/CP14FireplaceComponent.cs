namespace Content.Server._CP14.Temperature.Fireplace;

/// <summary>
/// component for player-controlled fire. Can be fueled.
/// </summary>

[RegisterComponent, Access(typeof(CP14FireplaceSystem))]
public sealed partial class CP14FireplaceComponent : Component
{
    /// <summary>
    /// The abstract amount of fuel that is used to keep a fire burning
    /// </summary>
    [DataField]
    public float MaxFuelLimit = 100f;

    /// <summary>
    /// how much the flame grows or dies out with the presence or absence of fuel
    /// </summary>
    [DataField]
    public float FireFadeDelta = 0.2f;

    /// <summary>
    /// current fuel quantity
    /// </summary>
    [DataField]
    public float CurrentFuel;

    /// <summary>
    /// how much fuel is wasted every "UpdateFrequency"
    /// </summary>
    [DataField]
    public float FuelDrainingPerUpdate = 1f;

    [DataField]
    public TimeSpan UpdateFrequency = TimeSpan.FromSeconds(2f);

    /// <summary>
    /// whether fuel can be added by hand
    /// </summary>
    [DataField]
    public bool CanInsertByHand = true;

    /// <summary>
    /// whether the fuel can be supplied by contact
    /// </summary>
    [DataField]
    public bool CanInsertByCollide = false;

    [DataField]
    public TimeSpan NextUpdateTime = TimeSpan.Zero;
}
