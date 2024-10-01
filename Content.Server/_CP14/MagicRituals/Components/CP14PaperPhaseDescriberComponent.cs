using Robust.Shared.Prototypes;

namespace Content.Server._CP14.MagicRituals.Components;

/// <summary>
///
/// </summary>
[RegisterComponent, Access(typeof(CP14RitualSystem))]
public sealed partial class CP14PaperPhaseDescriberComponent : Component
{
    [DataField]
    public EntProtoId? DescribePhase;
}

