using Content.Shared.FixedPoint;
using Content.Shared.Guidebook;

namespace Content.Shared._CP14.MagicEnergy.Components;

/// <summary>
/// Restores mana if the entity is in the sun, and wastes it if not
/// </summary>
[RegisterComponent, Access(typeof(SharedCP14MagicEnergySystem))]
public sealed partial class CP14MagicEnergyPhotosynthesisComponent : Component
{
    [DataField]
    [GuidebookData]
    public FixedPoint2 DaylightEnergy = 2f;

    [DataField]
    [GuidebookData]
    public FixedPoint2 DarknessEnergy = 0f;

    /// <summary>
    /// how often objects will try to change magic energy. In Seconds
    /// </summary>
    [DataField]
    public float Delay = 3f;

    [DataField]
    public float LightThreshold = 100f;

    /// <summary>
    /// the time of the next magic energy change
    /// </summary>
    [DataField]
    public TimeSpan NextUpdateTime { get; set; } = TimeSpan.Zero;
}
