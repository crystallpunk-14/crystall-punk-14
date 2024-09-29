using Content.Server._CP14.MagicRituals.Components.Triggers;
using Content.Server.Speech;
using Content.Server.Speech.Components;
using Content.Shared._CP14.MagicRitual;

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
    }

    #region Trigger timer

    private void OnTimerMapInit(Entity<CP14RitualTriggerTimerComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.TriggerTime = _timing.CurTime + TimeSpan.FromSeconds(ent.Comp.Time.Next(_random));
    }

    private void TimerUpdate()
    {
        var query = EntityQueryEnumerator<CP14RitualTriggerTimerComponent>();
        while (query.MoveNext(out var uid, out var timer))
        {
            if (_timing.CurTime < timer.TriggerTime)
                continue;

            timer.TriggerTime += TimeSpan.FromSeconds(timer.Time.Next(_random));

            var ev = new CP14RitualTriggerAttempt(timer.NextPhase);
            RaiseLocalEvent(uid, ev);
        }
    }

    #endregion

    #region Trigger voice

    private void OnVoiceInit(Entity<CP14RitualTriggerVoiceComponent> ent, ref ComponentInit args)
    {
        EnsureComp<ActiveListenerComponent>(ent).Range = ent.Comp.ListenRange;
    }

    private void OnListen(Entity<CP14RitualTriggerVoiceComponent> ent, ref ListenEvent args)
    {
        var message = args.Message.Trim();

        var triggered = false;
        foreach (var trigger in ent.Comp.NextPhases)
        {
            if (trigger.Key != message)
                continue;

            var triggerEv = new CP14RitualTriggerAttempt(trigger.Value);
            RaiseLocalEvent(ent, triggerEv);
            triggered = true;
            break;
        }

        if (triggered || ent.Comp.FailAttempts is null || ent.Comp.FailedPhase is null)
            return;

        ent.Comp.FailAttempts -= 1;

        if (!(ent.Comp.FailAttempts <= 0))
            return;

        var failEv = new CP14RitualTriggerAttempt(ent.Comp.FailedPhase.Value);
        RaiseLocalEvent(ent, failEv);
    }
    #endregion
}
