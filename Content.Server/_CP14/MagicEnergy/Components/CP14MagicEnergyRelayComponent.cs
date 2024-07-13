using Content.Shared._CP14.MagicEnergy.Components;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.MagicEnergy.Components;

/// <summary>
/// Allows you to constantly redirect energy to other entities within a radius.
/// </summary>
[RegisterComponent, Access(typeof(CP14MagicEnergySystem))]
public sealed partial class CP14MagicEnergyRelayComponent : Component
{
    [DataField]
    public float Radius = 5f;

    [DataField]
    public EntProtoId BeamProto = "CP14MagicBeam1";

    [DataField]
    public HashSet<EntityUid> Targets = new();
}
