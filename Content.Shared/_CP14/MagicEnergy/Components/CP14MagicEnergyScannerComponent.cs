using Content.Shared.Inventory;

namespace Content.Shared._CP14.MagicEnergy.Components;

/// <summary>
/// Allows you to see how much magic energy is stored in objects
/// </summary>
[RegisterComponent, Access(typeof(SharedCP14MagicEnergySystem))]
public sealed partial class CP14MagicEnergyScannerComponent : Component
{
}
