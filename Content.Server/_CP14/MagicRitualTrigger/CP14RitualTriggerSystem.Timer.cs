using Content.Shared._CP14.MagicRitual;
using Content.Shared._CP14.MagicRitualTrigger.Triggers;
using Robust.Shared.Timing;

namespace Content.Server._CP14.MagicRitualTrigger;


public partial class CP14RitualTriggerSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;

    private void InitializeTimer()
    {
        SubscribeLocalEvent<CP14RitualTimerTriggerComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<CP14RitualTimerTriggerComponent> ent, ref MapInitEvent args)
    {
        foreach (var trigger in ent.Comp.Triggers)
        {
            trigger.TriggerTime = _timing.CurTime + TimeSpan.FromSeconds(trigger.Delay);
        }
    }

    private void UpdateTimer(float frameTime)
    {
        var query = EntityQueryEnumerator<CP14RitualTimerTriggerComponent, CP14MagicRitualPhaseComponent>();
        while (query.MoveNext(out var uid, out var timer, out var phase))
        {
            foreach (var trigger in timer.Triggers)
            {
                if (_timing.CurTime < trigger.TriggerTime || trigger.TriggerTime == TimeSpan.Zero)
                    continue;

                if (trigger.Edge is null)
                    continue;

                TriggerRitualPhase((uid, phase), trigger.Edge.Value.Target);
                trigger.TriggerTime = TimeSpan.Zero;
            }
        }
    }
}
