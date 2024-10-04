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
        SubscribeLocalEvent<CP14MagicRitualComponent, ListenEvent>(OnListenEvent);
    }

    private void OnListenEvent(Entity<CP14MagicRitualComponent> ent, ref ListenEvent args)
    {
        if (ent.Comp.CurrentPhase is null)
            return;

        // Lowercase the phrase and remove all punctuation marks
        var message = Regex.Replace(args.Message.Trim().ToLower(), @"[^\w\s]", "");

        var phase = ent.Comp.CurrentPhase.Value;

        foreach (var edge in phase.Comp.Edges)
        {
            foreach (var trigger in edge.Triggers)
            {
                if (trigger is not VoiceTrigger voiceTrigger)
                    continue;

                var triggerMessage = Regex.Replace(voiceTrigger.Message.ToLower(), @"[^\w\s]", "");

                if (voiceTrigger.Message != args.Message)
                    continue;

                TriggerRitualPhase(phase, edge.Target);
            }
        }
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

                    TriggerRitualPhase((uid, phase), edge.Target);
                    timerTrigger.TriggerTime = TimeSpan.Zero;
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
