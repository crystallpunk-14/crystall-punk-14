using System.Text.RegularExpressions;
using Content.Server.Speech;
using Content.Shared._CP14.MagicRitual;
using Content.Shared._CP14.MagicRitual.Triggers;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.MagicRituals;

public sealed partial class CP14RitualSystem
{
    private void InitializeTriggers()
    {
        SubscribeLocalEvent<CP14RitualTimerTriggerComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<CP14RitualVoiceTriggerComponent, ListenEvent>(OnListenEvent);
    }

    private void OnMapInit(Entity<CP14RitualTimerTriggerComponent> ent, ref MapInitEvent args)
    {
        foreach (var trigger in ent.Comp.Triggers)
        {
            trigger.TriggerTime = _timing.CurTime + TimeSpan.FromSeconds(trigger.Delay);
        }
    }

    private void OnListenEvent(Entity<CP14RitualVoiceTriggerComponent> ent, ref ListenEvent args)
    {
        if (!TryComp<CP14MagicRitualPhaseComponent>(ent, out var phase))
            return;

        // Lowercase the phrase and remove all punctuation marks
        var message = Regex.Replace(args.Message.Trim().ToLower(), @"[^\w\s]", "");

        foreach (var trigger in ent.Comp.Triggers)
        {
            var triggerMessage = Regex.Replace(trigger.Message.ToLower(), @"[^\w\s]", "");

            if (triggerMessage != message)
                continue;

            if (trigger.Edge is null)
                continue;

            TriggerRitualPhase((ent.Owner, phase), trigger.Edge.Value.Target);
        }
    }

    private void UpdateTriggers(float frameTime)
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

    private void TriggerRitualPhase(Entity<CP14MagicRitualPhaseComponent> ent, EntProtoId nextPhase)
    {
        var evConfirmed = new CP14RitualTriggerEvent(nextPhase);
        RaiseLocalEvent(ent, evConfirmed);
    }
}
