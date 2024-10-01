using System.Text;
using Content.Server._CP14.MagicRituals.Components;
using Content.Server._CP14.MagicRituals.Components.Triggers;
using Content.Shared.Paper;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.MagicRituals;

public sealed partial class CP14RitualSystem
{
    [Dependency] private readonly PaperSystem _paper = default!;
    private void InitializeDescriber()
    {
        SubscribeLocalEvent<CP14PaperPhaseDescriberComponent, MapInitEvent>(OnMapInitDescriber);
    }

    private void OnMapInitDescriber(Entity<CP14PaperPhaseDescriberComponent> ent, ref MapInitEvent args)
    {
        if (ent.Comp.DescribePhase is null)
            return;

        if (!TryComp<PaperComponent>(ent, out var paper))
            return;

        _paper.SetContent((ent, paper), GetPhaseDescription(ent.Comp.DescribePhase.Value) ?? "");
    }

    public string? GetPhaseDescription(EntProtoId proto)
    {
        if (!_proto.TryIndex(proto, out var _))
            return null;

        var tmp = Spawn(proto, MapCoordinates.Nullspace);

        if (!TryComp<CP14MagicRitualPhaseComponent>(tmp, out var phase))
            return null;

        Entity<CP14MagicRitualPhaseComponent> ent = (tmp, phase);


        var sb = new StringBuilder();
        sb.Append("\n");
        sb.Append($"[color=#e6a132][head=2]{MetaData(ent).EntityName}[/head][/color] \n");
        sb.Append($"[italic]{MetaData(ent).EntityDescription}[/italic] \n");

        sb.Append(Loc.GetString("cp14-ritual-intro") + "\n \n");
        foreach (var edge in ent.Comp.Edges)
        {
            if (!_proto.TryIndex(edge.Target, out var targetIndexed))
                continue;

            sb.Append($"[color=#b5783c][head=2]{targetIndexed.Name}[/head][/color]" + "\n");
            if (edge.Requirements.Count > 0)
            {
                sb.Append($"[bold]{Loc.GetString("cp14-ritual-req-header")}[/bold] \n");
                foreach (var req in edge.Requirements)
                    sb.Append(req.GetGuidebookRequirementDescription(_proto, _entitySystem));
                sb.Append("\n");
            }

            if (edge.Actions.Count > 0)
            {
                sb.Append($"[bold]{Loc.GetString("cp14-ritual-effect-header")}[/bold] \n");
                foreach (var act in edge.Actions)
                    sb.Append(act.GetGuidebookEffectDescription(_proto, _entitySystem));
                sb.Append("\n");
            }
            sb.Append("\n");
        }
        sb.Append($"[head=2]{Loc.GetString("cp14-ritual-trigger-header")}[/head] \n");

        var triggersExist = false;
        if (TryComp<CP14RitualTriggerTimerComponent>(ent, out var timer) &&
            _proto.TryIndex(timer.NextPhase, out var indexedTimerPhase))
        {
            if (timer.Time.Min == timer.Time.Max)
                sb.Append(Loc.GetString("cp14-ritual-trigger-timer-stable", ("node", indexedTimerPhase.Name), ("time", timer.Time.Max)) + "\n");
            else
                sb.Append(Loc.GetString("cp14-ritual-trigger-timer-unstable", ("node", indexedTimerPhase.Name), ("min", timer.Time.Min), ("max", timer.Time.Max))+ "\n");

            triggersExist = true;
        }

        if (TryComp<CP14RitualTriggerVoiceComponent>(ent, out var voice))
        {
            foreach (var trigger in voice.Triggers)
            {
                if (!_proto.TryIndex(trigger.TargetPhase, out var indexedVoicePhase))
                    continue;

                sb.Append(Loc.GetString("cp14-ritual-trigger-voice",
                    ("phrase", trigger.Message),
                    ("range", voice.ListenRange),
                    ("count", trigger.UniqueSpeakers),
                    ("node", indexedVoicePhase.Name)) + "\n");
            }

            if (voice.FailAttempts is not null &&
                voice.FailedPhase is not null &&
                _proto.TryIndex(voice.FailedPhase, out var indexedVoiceFailPhase))
            {
                sb.Append(Loc.GetString("cp14-ritual-trigger-voice-limits", ("node", indexedVoiceFailPhase.Name)) + "\n");
            }

            triggersExist = true;
        }

        if (!triggersExist)
            sb.Append($"{Loc.GetString("cp14-ritual-trigger-none")} \n");

        return sb.ToString();
    }
}
