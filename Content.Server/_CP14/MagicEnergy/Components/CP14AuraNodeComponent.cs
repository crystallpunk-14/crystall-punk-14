using Content.Shared.FixedPoint;

namespace Content.Server._CP14.MagicEnergy.Components;

[RegisterComponent, Access(typeof(CP14MagicEnergySystem))]
public sealed partial class CP14AuraNodeComponent : Component
{
    [DataField]
    public bool Enable = true;

    [DataField]
    public FixedPoint2 Energy = 1f;

    [DataField]
    public float Range = 10f;

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

[RegisterComponent, Access(typeof(CP14MagicEnergySystem))]
public sealed partial class CP14RandomAuraNodeComponent : Component
{
    [DataField]
    public float MinDraw = -2f;

    [DataField]
    public float MaxDraw = 2f;

    [DataField]
    public float MinRange = 5f;

    [DataField]
    public float MaxRange = 10f;
}
