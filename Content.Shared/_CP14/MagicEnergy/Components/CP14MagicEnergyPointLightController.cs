using Content.Shared.Inventory;

namespace Content.Shared._CP14.MagicEnergy.Components;

/// <summary>
/// Controls the strength of the PointLight component, depending on the amount of mana in the object
/// </summary>
[RegisterComponent, Access(typeof(SharedCP14MagicEnergySystem))]
public sealed partial class CP14MagicEnergyPointLightControllerComponent : Component
{
    [DataField]
    public float MaxEnergy = 1f;

    [DataField]
    public float MinEnergy = 0f;
}
