using System.Text.RegularExpressions;
using Content.Server._CP14.MagicRituals.Components;
using Content.Server._CP14.MagicRituals.Components.Triggers;
using Content.Server.Speech;
using Content.Server.Speech.Components;
using Content.Shared._CP14.MagicRitual;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.MagicRituals;

public sealed partial class CP14RitualSystem
{
    private void InitializeTriggers()
    {
        SubscribeLocalEvent<CP14RitualTriggerVoiceComponent, ComponentInit>(OnVoiceInit);
        SubscribeLocalEvent<CP14RitualTriggerVoiceComponent, ListenEvent>(OnListen);

        SubscribeLocalEvent<CP14RitualTriggerTimerComponent, MapInitEvent>(OnTimerMapInit);
    }

    private void UpdateTriggers(float frameTime)
    {
        TimerUpdate();
        VoiceUpdate();
    }

    private void TriggerRitualPhase(Entity<CP14MagicRitualPhaseComponent> ent, EntProtoId nextPhase)
    {
        var evConfirmed = new CP14RitualTriggerEvent(nextPhase);
        RaiseLocalEvent(ent, evConfirmed);
    }

    #region Trigger timer

    private void OnTimerMapInit(Entity<CP14RitualTriggerTimerComponent> ent, ref MapInitEvent args)
    {
        var time = ent.Comp.Time.Next(_random);
        ent.Comp.TriggerTime = _timing.CurTime + TimeSpan.FromSeconds(time);
    }

    private void TimerUpdate()
    {
        var query = EntityQueryEnumerator<CP14RitualTriggerTimerComponent, CP14MagicRitualPhaseComponent>();
        while (query.MoveNext(out var uid, out var timer, out var phase))
        {
            if (_timing.CurTime < timer.TriggerTime)
                continue;

            timer.TriggerTime += TimeSpan.FromSeconds(timer.Time.Next(_random));

            TriggerRitualPhase((uid, phase), timer.NextPhase);
        }
    }

    #endregion

    #region Trigger voice

    private void VoiceUpdate()
    {
        var query = EntityQueryEnumerator<CP14RitualTriggerVoiceComponent, CP14MagicRitualPhaseComponent>();
        while (query.MoveNext(out var uid, out var voice, out var phase))
        {
            if (voice.EndWindowTime == TimeSpan.Zero)
                continue;

            if (_timing.CurTime < voice.EndWindowTime)
                continue;

            voice.EndWindowTime = TimeSpan.Zero;
            voice.UniqueSpeakersCount.Clear();
            VoiceTriggerFailAttempt((uid, voice), phase);
        }
    }
    private void OnVoiceInit(Entity<CP14RitualTriggerVoiceComponent> ent, ref ComponentInit args)
    {
        EnsureComp<ActiveListenerComponent>(ent).Range = ent.Comp.ListenRange;
    }

    private void OnListen(Entity<CP14RitualTriggerVoiceComponent> ent, ref ListenEvent args)
    {
        if (!TryComp<CP14MagicRitualPhaseComponent>(ent, out var phase))
            return;

        // Lowercase the phrase and remove all punctuation marks
        var message = Regex.Replace(args.Message.Trim().ToLower(), @"[^\w\s]", "");

        var triggered = false;
        foreach (var trigger in ent.Comp.Triggers)
        {
            var triggerMessage = Regex.Replace(trigger.Message.ToLower(), @"[^\w\s]", "");

            if (triggerMessage != message)
                continue;

            triggered = true;

            if (trigger.UniqueSpeakers > 1)
            {
                // Add new speaker (ignore repeating)
                if (ent.Comp.UniqueSpeakersCount.Contains(args.Source))
                {
                    VoiceTriggerFailAttempt(ent, phase);
                    break;
                }

                ent.Comp.UniqueSpeakersCount.Add(args.Source);

                //If first - start timer
                if (ent.Comp.UniqueSpeakersCount.Count == 1)
                    ent.Comp.EndWindowTime = _timing.CurTime + ent.Comp.WindowSize;

                if (ent.Comp.UniqueSpeakersCount.Count < trigger.UniqueSpeakers)
                    continue;
            }

            TriggerRitualPhase((ent.Owner,phase), trigger.TargetPhase);
            break;
        }

        if (!triggered)
            VoiceTriggerFailAttempt(ent, phase);
    }

    private void VoiceTriggerFailAttempt(Entity<CP14RitualTriggerVoiceComponent> ent, CP14MagicRitualPhaseComponent phase)
    {
        if (ent.Comp.FailAttempts is null)
            return;

        ent.Comp.FailAttempts -= 1;

        if (phase.Ritual is not null)
            ChangeRitualStability(phase.Ritual.Value, -ent.Comp.FailTriggerStabilityCost);

        if (ent.Comp.FailAttempts <= 0 && ent.Comp.FailedPhase is not null)
            TriggerRitualPhase((ent.Owner,phase), ent.Comp.FailedPhase.Value);
    }
    #endregion
}
