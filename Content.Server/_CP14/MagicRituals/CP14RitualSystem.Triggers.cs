using Content.Shared._CP14.MagicRitual;
using Content.Shared._CP14.MagicRitual.Triggers;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.MagicRituals;

public sealed partial class CP14RitualSystem
{
    private void InitializeTriggers()
    {
    }

    private void UpdateTriggers(float frameTime)
    {
        var query = EntityQueryEnumerator<CP14MagicRitualPhaseComponent>();
        while (query.MoveNext(out var uid, out var phase))
        {
            foreach (var edge in phase.Edges)
            {
                foreach (var trigger in edge.Triggers)
                {
                    if (trigger is not TimerTrigger timerTrigger)
                        continue;

                    if (_timing.CurTime < timerTrigger.TriggerTime)
                        continue;

                    TriggerRitualPhase((uid, phase), timerTrigger.Phase);
                }
            }
        }
    }

    private void TriggerRitualPhase(Entity<CP14MagicRitualPhaseComponent> ent, EntProtoId nextPhase)
    {
        var evConfirmed = new CP14RitualTriggerEvent(nextPhase);
        RaiseLocalEvent(ent, evConfirmed);
    }
}
