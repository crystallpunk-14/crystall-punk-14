namespace Content.Shared._CP14.MagicRitual.Triggers;

/// <summary>
///
/// </summary>
[RegisterComponent]
public sealed partial class CP14RitualVoiceTriggerComponent : Component
{
    [DataField]
    public HashSet<CP14VoiceTrigger> Triggers = new();
}
