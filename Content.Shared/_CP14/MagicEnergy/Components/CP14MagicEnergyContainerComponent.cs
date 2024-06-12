namespace Content.Shared._CP14.MagicEnergy.Components;

/// <summary>
/// Allows an item to store magical energy within itself.
/// </summary>
[RegisterComponent, Access(typeof(SharedCP14MagicEnergySystem))]
public sealed partial class CP14MagicEnergyContainerComponent : Component
{
    [DataField]
    public float Energy = 0f;

    [DataField]
    public float MaxEnergy = 100f;
}
