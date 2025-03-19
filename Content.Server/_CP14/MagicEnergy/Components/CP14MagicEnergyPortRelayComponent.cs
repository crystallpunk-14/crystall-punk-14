using Content.Shared.DeviceLinking;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.MagicEnergy.Components;

/// <summary>
/// Allows you to relay magical energy to other objects through signal sustem
/// </summary>
[RegisterComponent, Access(typeof(CP14MagicEnergySystem))]
public sealed partial class CP14MagicEnergyPortRelayComponent : Component
{
    [DataField]
    public ProtoId<SinkPortPrototype>? SinkPort = CP14MagicEnergySystem.PowerSinkPort;

    [DataField]
    public ProtoId<SourcePortPrototype>? SourcePort = CP14MagicEnergySystem.PowerSourcePort;

    [DataField]
    public bool Enable = true;

    [DataField]
    public bool Safe = true;

    [DataField]
    public float Delay = 5f;

    [DataField]
    public FixedPoint2 Energy = 5f;

    [DataField]
    public TimeSpan NextUpdateTime { get; set; } = TimeSpan.Zero;
}
