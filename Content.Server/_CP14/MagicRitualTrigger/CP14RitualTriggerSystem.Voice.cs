using System.Text.RegularExpressions;
using Content.Server.Speech;
using Content.Shared._CP14.MagicRitual;
using Content.Shared._CP14.MagicRitualTrigger.Triggers;

namespace Content.Server._CP14.MagicRitualTrigger;

public partial class CP14RitualTriggerSystem
{
    private void InitializeVoice()
    {
        SubscribeLocalEvent<CP14RitualVoiceTriggerComponent, ListenEvent>(OnListenEvent);
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
}
