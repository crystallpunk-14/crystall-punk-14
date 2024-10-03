using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared._CP14.MagicRitual.Triggers;

/// <summary>
/// Requires that the stability of the ritual be within specified limits. If the stability is above or below the specified values, the check will fail
/// </summary>
public sealed partial class TimerTrigger : CP14RitualTrigger
{
    [DataField]
    public float Delay = 10f;

    [DataField]
    public TimeSpan TriggerTime = TimeSpan.Zero;


    public override void Initialize(EntityManager entManager, Entity<CP14MagicRitualPhaseComponent> phaseEnt)
    {
        var _gameTiming = IoCManager.Resolve<IGameTiming>();

        TriggerTime = _gameTiming.CurTime + TimeSpan.FromSeconds(Delay);
    }

    public override string? GetGuidebookTriggerDescription(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        return null;
    }
}
