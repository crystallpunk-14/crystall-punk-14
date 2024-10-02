using Content.Shared.Destructible.Thresholds;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.MagicRituals.Components.Triggers;

/// <summary>
///
/// </summary>
[RegisterComponent, Access(typeof(CP14RitualSystem))]
public sealed partial class CP14RitualTriggerTimerComponent : Component
{
    [DataField(required: true)]
    public MinMax Time = new MinMax(0, 1);

    [DataField(required: true)]
    public EntProtoId NextPhase;
}
