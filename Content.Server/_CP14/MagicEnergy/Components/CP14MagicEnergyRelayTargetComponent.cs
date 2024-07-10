namespace Content.Server._CP14.MagicEnergy.Components;

/// <summary>
/// Allows you to constantly redirect energy to other entities within a radius.
/// </summary>
[RegisterComponent, Access(typeof(CP14MagicEnergySystem))]
public sealed partial class CP14MagicEnergyRelayTargetComponent : Component
{
    [DataField]
    public HashSet<EntityUid> Source = new();
}
