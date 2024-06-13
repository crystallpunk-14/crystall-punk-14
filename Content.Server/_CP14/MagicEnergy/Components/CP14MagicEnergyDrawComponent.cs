using Content.Shared.FixedPoint;

namespace Content.Server._CP14.MagicEnergy.Components;

/// <summary>
/// Allows you to restore or deplete the magical energy in the item
/// </summary>
[RegisterComponent, Access(typeof(CP14MagicEnergySystem))]
public sealed partial class CP14MagicEnergyDrawComponent : Component
{
    [DataField]
    public bool Enable = true;

    [DataField]
    public FixedPoint2 Energy = 1f;

    /// <summary>
    /// If not safe, restoring or drawing power across boundaries call dangerous events, that may destroy crystals
    /// </summary>
    [DataField]
    public bool Safe = true;

    /// <summary>
    /// how often objects will try to change magic energy. In Seconds
    /// </summary>
    [DataField]
    public float Delay = 5f;

    /// <summary>
    /// the time of the next magic energy change
    /// </summary>
    [DataField]
    public TimeSpan NextUpdateTime { get; set; } = TimeSpan.Zero;
}
