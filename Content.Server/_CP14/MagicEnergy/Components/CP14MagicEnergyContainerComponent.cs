namespace Content.Server._CP14.MagicEnergy.Components;

/// <summary>
/// allows the object to become blunt with use
/// </summary>
[RegisterComponent, Access(typeof(CP14MagicEnergySystem))]
public sealed partial class CP14MagicEnergyContainerComponent : Component
{
    [DataField]
    public float Energy = 0f;

    [DataField]
    public float MaxEnergy = 100f;
}
