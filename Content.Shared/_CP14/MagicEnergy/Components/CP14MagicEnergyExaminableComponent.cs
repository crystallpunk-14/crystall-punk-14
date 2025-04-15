using Robust.Shared.GameStates;

namespace Content.Shared._CP14.MagicEnergy.Components;

/// <summary>
/// Allows you to examine how much energy is in that object.
/// </summary>
[RegisterComponent, NetworkedComponent]
[Access(typeof(SharedCP14MagicEnergySystem))]
public sealed partial class CP14MagicEnergyExaminableComponent : Component;
