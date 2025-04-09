using Content.Shared.Alert;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;
using Robust.Shared.GameStates;

namespace Content.Shared._CP14.MagicEnergy.Components;

/// <summary>
/// Allows an item to store magical energy within itself.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedCP14MagicEnergySystem))]
public sealed partial class CP14MagicEnergyContainerComponent : Component
{
    [DataField, AutoNetworkedField]
    public FixedPoint2 Energy = 0f;

    [DataField, AutoNetworkedField]
    public FixedPoint2 MaxEnergy = 100f;

    [DataField, AutoNetworkedField]
    public ProtoId<AlertPrototype>? MagicAlert = null;

    /// <summary>
    /// Does this container support unsafe energy manipulation?
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool UnsafeSupport = false;
}
