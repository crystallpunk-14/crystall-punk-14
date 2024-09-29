using Robust.Shared.Prototypes;

namespace Content.Server._CP14.MagicRituals.Components;

/// <summary>
/// Ritual Behavior Controller. Creates and removes entities of magical phases
/// </summary>
[RegisterComponent, Access(typeof(CP14RitualSystem))]
public sealed partial class CP14MagicRitualComponent : Component
{
    [DataField(required: true)]
    public EntProtoId StartPhase;

    /// <summary>
    ///
    /// </summary>
    [DataField]
    public Entity<CP14MagicRitualPhaseComponent>? CurrentPhase;
}
