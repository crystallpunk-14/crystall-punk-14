namespace Content.Shared._CP14.MagicRitualTrigger.Triggers;

[RegisterComponent]
public sealed partial class CP14RitualTimerTriggerComponent : Component
{
    [DataField]
    public HashSet<CP14TimerTrigger> Triggers = new();
}
