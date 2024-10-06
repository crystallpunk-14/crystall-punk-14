using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.MagicRitual.Triggers;

/// <summary>
///
/// </summary>
[RegisterComponent]
public sealed partial class CP14RitualTimerTriggerComponent : Component
{
    [DataField]
    public HashSet<CP14TimerTrigger> Triggers = new();
}
