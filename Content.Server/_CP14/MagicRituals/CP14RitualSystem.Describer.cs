using System.Text;
using Content.Server._CP14.MagicRituals.Components;
using Content.Server._CP14.MagicRituals.Components.Triggers;
using Content.Shared._CP14.MagicRitual;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Paper;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.MagicRituals;

public sealed partial class CP14RitualSystem
{
    [Dependency] private readonly PaperSystem _paper = default!;
    private void InitializeDescriber()
    {
        SubscribeLocalEvent<CP14PaperPhaseDescriberComponent, AfterInteractEvent>(OnDescriberAfterInteract);
        SubscribeLocalEvent<CP14PaperPhaseDescriberComponent, CP14DescribeRitualDoAfter>(OnFinishDescribe);
    }

    private void OnDescriberAfterInteract(Entity<CP14PaperPhaseDescriberComponent> ent, ref AfterInteractEvent args)
    {
        if (!TryComp<CP14MagicRitualComponent>(args.Target, out var ritual))
            return;

        var doAfterArgs =
            new DoAfterArgs(EntityManager, args.User, ent.Comp.DescribeTime, new CP14DescribeRitualDoAfter(), ent, args.Target)
            {
                BreakOnDamage = true,
                BreakOnMove = true,
            };

        _doAfter.TryStartDoAfter(doAfterArgs);
    }

    private void OnFinishDescribe(Entity<CP14PaperPhaseDescriberComponent> ent, ref CP14DescribeRitualDoAfter args)
    {
        if (args.Cancelled || args.Handled)
            return;
        args.Handled = true;

        if (!TryComp<CP14MagicRitualComponent>(args.Target, out var ritual))
            return;

        if (!TryComp<PaperComponent>(ent, out var paper))
            return;

        if (ritual.CurrentPhase is null)
            return;

        _paper.SetContent((ent, paper), GetPhaseDescription(ritual.CurrentPhase.Value) ?? "");
    }

    public string? GetPhaseDescription(Entity<CP14MagicRitualPhaseComponent> ent)
    {
        GetTriggers(ent, out var triggerList);

        var sb = new StringBuilder();
        sb.Append($"[color=#e6a132][head=1]{MetaData(ent).EntityName}[/head][/color] \n \n");
        sb.Append($"[italic]{MetaData(ent).EntityDescription}[/italic] \n \n");

        sb.Append(Loc.GetString("cp14-ritual-intro") + "\n \n \n");
        foreach (var edge in ent.Comp.Edges)
        {
            if (!_proto.TryIndex(edge.Target, out var targetIndexed))
                continue;

            sb.Append($"[color=#b5783c][head=3]{targetIndexed.Name}[/head][/color]" + "\n");
            //TRIGGERS
            foreach (var trigger in triggerList)
            {
                if (trigger.Item1 != edge.Target)
                    continue;
                sb.Append($"[bold]{Loc.GetString("cp14-ritual-trigger-header")}[/bold] \n");
                sb.Append(trigger.Item2);
            }

            //REQUIREMENTS
            if (edge.Requirements.Count > 0)
            {
                sb.Append($"[bold]{Loc.GetString("cp14-ritual-req-header")}[/bold] \n");
                foreach (var req in edge.Requirements)
                    sb.Append(req.GetGuidebookRequirementDescription(_proto, _entitySystem));
                sb.Append("\n");
            }

            //ACTIONS
            if (edge.Actions.Count > 0)
            {
                sb.Append($"[bold]{Loc.GetString("cp14-ritual-effect-header")}[/bold] \n");
                foreach (var act in edge.Actions)
                    sb.Append(act.GetGuidebookEffectDescription(_proto, _entitySystem));
                sb.Append("\n");
            }
        }
        return sb.ToString();
    }

    private void GetTriggers(Entity<CP14MagicRitualPhaseComponent> ent, out List<(EntProtoId, string)> triggerList)
    {
        triggerList = new();

        if (TryComp<CP14RitualTriggerTimerComponent>(ent, out var timer))
        {
            if (timer.Time.Min == timer.Time.Max)
            {
                triggerList.Add((
                    timer.NextPhase,
                    Loc.GetString("cp14-ritual-trigger-timer-stable",
                        ("time", timer.Time.Max)) + "\n"));
            }
            else
            {
                triggerList.Add((
                    timer.NextPhase,
                    Loc.GetString("cp14-ritual-trigger-timer-unstable",
                        ("min", timer.Time.Min),
                        ("max", timer.Time.Max)) + "\n"));
            }
        }

        if (TryComp<CP14RitualTriggerVoiceComponent>(ent, out var voice))
        {
            foreach (var trigger in voice.Triggers)
            {
                triggerList.Add((trigger.TargetPhase, Loc.GetString("cp14-ritual-trigger-voice",
                    ("phrase", trigger.Message),
                    ("range", voice.ListenRange),
                    ("count", trigger.Speakers)) + "\n"));
            }

            if (voice.FailAttempts is not null && voice.FailedPhase is not null)
            {
                triggerList.Add((voice.FailedPhase.Value,
                    Loc.GetString("cp14-ritual-trigger-voice-limits") + "\n"));
            }
        }
    }

    public string? GetPhaseDescription(EntProtoId proto)
    {
        if (!_proto.TryIndex(proto, out _))
            return null;

        var tmp = Spawn(proto, MapCoordinates.Nullspace);

        if (!TryComp<CP14MagicRitualPhaseComponent>(tmp, out var phase))
            return null;

        Entity<CP14MagicRitualPhaseComponent> ent = (tmp, phase);

        return GetPhaseDescription(ent);
    }
}
