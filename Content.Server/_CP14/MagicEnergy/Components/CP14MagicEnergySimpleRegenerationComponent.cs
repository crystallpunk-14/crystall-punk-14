using Content.Shared.FixedPoint;

namespace Content.Server._CP14.MagicEnergy.Components;

/// <summary>
/// Allows you to restore or deplete the magical energy in the item
/// </summary>
[RegisterComponent, Access(typeof(CP14MagicEnergySystem))]
public sealed partial class CP14MagicEnergySimpleRegenerationComponent : Component
{
    [DataField]
    public FixedPoint2 Energy = 1f;

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
