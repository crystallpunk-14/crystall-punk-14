using Robust.Shared.Prototypes;

namespace Content.Server._CP14.MagicRituals.Components.Triggers;

/// <summary>
///
/// </summary>
[RegisterComponent, Access(typeof(CP14RitualSystem))]
public sealed partial class CP14RitualTriggerVoiceComponent : Component
{
    [DataField]
    public float ListenRange = 5f;

    [DataField(required: true)]
    public Dictionary<string, EntProtoId> NextPhases = new();

    /// <summary>
    /// the number of errors (incorrect phrases) that can be said next to the ritual, before switching to FailedPhase.
    /// set null, if you want an infinite number of tries
    /// </summary>
    [DataField]
    public int? FailAttempts = null;

    [DataField]
    public EntProtoId? FailedPhase;
}
