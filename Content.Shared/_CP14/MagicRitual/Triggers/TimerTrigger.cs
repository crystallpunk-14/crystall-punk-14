using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared._CP14.MagicRitual.Triggers;

/// <summary>
/// Triggers the phase transition after a certain period of time
/// </summary>
public sealed partial class TimerTrigger : CP14RitualTrigger
{
    [DataField]
    public float Delay = 10f;

    [DataField]
    public TimeSpan TriggerTime = TimeSpan.Zero;

    public override void Initialize(EntityManager entManager, Entity<CP14MagicRitualComponent> ritual)
    {
        var gameTiming = IoCManager.Resolve<IGameTiming>();

        TriggerTime = gameTiming.CurTime + TimeSpan.FromSeconds(Delay);
    }

    public override string? GetGuidebookTriggerDescription(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        return Loc.GetString("cp14-ritual-trigger-timer-stable", ("time", Delay));
    }
}
