using Content.Server._CP14.MagicRituals.Components;
using Content.Server._CP14.MagicRituals.Components.Triggers;
using Content.Server.Speech;
using Content.Server.Speech.Components;
using Content.Shared._CP14.MagicRitual;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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
    }

    private void TriggerRitualPhase(Entity<CP14MagicRitualPhaseComponent> ent, EntProtoId nextPhase)
    {
        var evConfirmed = new CP14RitualTriggerEvent(nextPhase);
        RaiseLocalEvent(ent, evConfirmed);
    }

    #region Trigger timer

    private void OnTimerMapInit(Entity<CP14RitualTriggerTimerComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.TriggerTime = _timing.CurTime + TimeSpan.FromSeconds(ent.Comp.Time.Next(_random));
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

    private void OnVoiceInit(Entity<CP14RitualTriggerVoiceComponent> ent, ref ComponentInit args)
    {
        EnsureComp<ActiveListenerComponent>(ent).Range = ent.Comp.ListenRange;
    }

    private void OnListen(Entity<CP14RitualTriggerVoiceComponent> ent, ref ListenEvent args)
    {
        if (!TryComp<CP14MagicRitualPhaseComponent>(ent, out var phase))
            return;

        var message = args.Message.Trim();

        var triggered = false;
        foreach (var trigger in ent.Comp.NextPhases)
        {
            if (trigger.Key != message)
                continue;

            TriggerRitualPhase((ent.Owner,phase), trigger.Value);
            triggered = true;
            break;
        }

        if (triggered || ent.Comp.FailAttempts is null || ent.Comp.FailedPhase is null)
            return;

        ent.Comp.FailAttempts -= 1;

        if (!(ent.Comp.FailAttempts <= 0))
            return;

        TriggerRitualPhase((ent.Owner,phase), ent.Comp.FailedPhase.Value);
    }
    #endregion
}
