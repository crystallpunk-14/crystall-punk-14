using Content.Shared._CP14.MagicEnergy;

namespace Content.Server._CP14.MagicEnergy.Components;

/// <summary>
///
/// </summary>
[RegisterComponent, Access(typeof(SharedCP14MagicEnergySystem))]
public sealed partial class CP14PowerlineGauntletComponent : Component
{
    [DataField]
    public Entity<CP14MagicEnergyRelayComponent>? LinkedRelay;
}
