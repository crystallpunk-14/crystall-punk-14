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
    public List<TriggerVoiceData> Triggers = new();

    /// <summary>
    /// the number of errors (incorrect phrases) that can be said next to the ritual, before switching to FailedPhase.
    /// set null, if you want an infinite number of tries
    /// </summary>
    [DataField]
    public int? FailAttempts = null;

    [DataField]
    public EntProtoId? FailedPhase;
}

[DataRecord]
public partial record struct TriggerVoiceData()
{
    public EntProtoId TargetPhase { get; set; } = default!;

    public string Message { get; set; } = "null";

    /// <summary>
    /// can require several different entities to speak a phrase at the same time.
    /// </summary>
    public int UniqueSpeakers { get; set; } = 1;
}

