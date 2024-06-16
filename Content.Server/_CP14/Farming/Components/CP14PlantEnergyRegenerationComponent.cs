namespace Content.Server._CP14.Farming.Components;

/// <summary>
/// allows the plant to receive energy passively, depending on daylighting
/// </summary>
[RegisterComponent, Access(typeof(CP14FarmingSystem))]
public sealed partial class CP14PlantEnergyRegenerationComponent : Component
{
    [DataField]
    public float Energy = 1f;

    [DataField]
    public bool OnDaylight = true;

    [DataField]
    public bool InDark = false;

    [DataField]
    public float MinUpdateFrequency = 30f;

    [DataField]
    public float MaxUpdateFrequency = 90f;

    [DataField]
    public TimeSpan NextUpdateTime = TimeSpan.Zero;
}
