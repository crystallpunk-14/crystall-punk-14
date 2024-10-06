using Content.Shared._CP14.MagicRitual;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.MagicRitualTrigger.Triggers;

/// <summary>
/// Triggers the phase transition after a certain period of time
/// </summary>
public sealed partial class CP14TimerTrigger : CP14RitualTrigger
{
    [DataField]
    public float Delay = 10f;

    [DataField]
    public TimeSpan TriggerTime = TimeSpan.Zero;

    public override void Initialize(EntityManager entManager, Entity<CP14MagicRitualPhaseComponent> ritual, RitualPhaseEdge edge)
    {
        entManager.EnsureComponent<CP14RitualTimerTriggerComponent>(ritual, out var trigger);
        trigger.Triggers.Add(this);
        Edge = edge;
    }

    public override string? GetGuidebookTriggerDescription(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        return Loc.GetString("cp14-ritual-trigger-timer-stable", ("time", Delay));
    }
}
