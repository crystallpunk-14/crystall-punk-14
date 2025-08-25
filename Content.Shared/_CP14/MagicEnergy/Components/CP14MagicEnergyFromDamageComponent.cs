using Content.Shared.Damage.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.MagicEnergy.Components;

/// <summary>
/// Restores or expends magical energy when taking damage of certain types.
/// </summary>
[RegisterComponent, Access(typeof(CP14SharedMagicEnergySystem))]
public sealed partial class CP14MagicEnergyFromDamageComponent : Component
{
    [DataField]
    public Dictionary<ProtoId<DamageTypePrototype>, float> Damage = new();
}
