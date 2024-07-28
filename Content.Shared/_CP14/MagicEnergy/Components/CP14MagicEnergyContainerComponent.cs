using Content.Shared.Alert;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.MagicEnergy.Components;

/// <summary>
/// Allows an item to store magical energy within itself.
/// </summary>
[RegisterComponent, Access(typeof(SharedCP14MagicEnergySystem))]
public sealed partial class CP14MagicEnergyContainerComponent : Component
{
    [DataField]
    public FixedPoint2 Energy = 0f;

    [DataField]
    public FixedPoint2 MaxEnergy = 100f;

    [DataField]
    public ProtoId<AlertPrototype>? MagicAlert = null;
}
