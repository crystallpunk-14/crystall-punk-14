using System.Text;
using Content.Server._CP14.MagicRituals.Components;
using Content.Shared._CP14.MagicRitual;
using Content.Shared.Paper;
using Content.Shared.Verbs;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.MagicRituals;

public sealed partial class CP14RitualSystem
{
    [Dependency] private readonly PaperSystem _paper = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    private void InitializeDescriber()
    {

        SubscribeLocalEvent<CP14PaperPhaseDescriberComponent, GetVerbsEvent<Verb>>(OnDescriberVerbs);
        SubscribeLocalEvent<CP14PaperPhaseDescriberComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<CP14PaperPhaseDescriberComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnShutdown(Entity<CP14PaperPhaseDescriberComponent> ent, ref ComponentShutdown args)
    {
        QueueDel(ent.Comp.CurrentPhase);
    }

    private void OnMapInit(Entity<CP14PaperPhaseDescriberComponent> ent, ref MapInitEvent args)
    {
        SetPhase(ent, ent.Comp.StartPhase);
    }

    private void SetPhase(Entity<CP14PaperPhaseDescriberComponent> ent, EntProtoId newProto, bool saveHistory = true)
    {
        var oldPhase = ent.Comp.CurrentPhase;
        if (oldPhase is not null && saveHistory)
        {
            var oldProto = MetaData(oldPhase.Value).EntityPrototype;
            if (oldProto is not null && oldProto != newProto)
            {
                ent.Comp.SearchHistory.Add(oldProto);
                if (ent.Comp.SearchHistory.Count > 50)
                    ent.Comp.SearchHistory.RemoveAt(0);
            }
        }
        QueueDel(oldPhase);
        var newPhase = Spawn(newProto, MapCoordinates.Nullspace);

        ent.Comp.CurrentPhase = newPhase;

        if (!TryComp<PaperComponent>(ent, out var paper))
            return;

        _paper.SetContent((ent, paper), GetPhaseDescription(newPhase));
        _audio.PlayPvs(ent.Comp.UseSound, ent);
    }

    private void BackPhase(Entity<CP14PaperPhaseDescriberComponent> ent)
    {
        if (ent.Comp.SearchHistory.Count > 0)
            SetPhase(ent, ent.Comp.SearchHistory[^1], false);
    }

    private void OnDescriberVerbs(Entity<CP14PaperPhaseDescriberComponent> ent, ref GetVerbsEvent<Verb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        if (!TryComp<CP14MagicRitualPhaseComponent>(ent.Comp.CurrentPhase, out var phase))
            return;

        if (!TryComp<CP14MagicRitualPhaseComponent>(ent.Comp.CurrentPhase.Value, out var phaseComp))
            return;

        foreach (var edge in phaseComp.Edges)
        {
            if (!_proto.TryIndex(edge.Target, out var indexedTarget))
                continue;

            Verb verb = new()
            {
                Text = Loc.GetString("cp14-ritual-describer-verb-item", ("name", indexedTarget.Name)),
                Category = VerbCategory.CP14RitualBook,
                Priority = 1,
                Act = () => SetPhase(ent, edge.Target),
            };
            args.Verbs.Add(verb);
        }

        foreach (var hyperlink in ent.Comp.Hyperlinks)
        {
            if (!_proto.TryIndex(hyperlink, out var indexedTarget))
                continue;

            Verb verb = new()
            {
                Text = Loc.GetString("cp14-ritual-describer-verb-hyperlink", ("name", indexedTarget.Name)),
                Category = VerbCategory.CP14RitualBook,
                Priority = 0,
                Act = () => SetPhase(ent, hyperlink),
            };
            args.Verbs.Add(verb);
        }

        if (ent.Comp.SearchHistory.Count > 0)
        {
            Verb verb = new()
            {
                Text = Loc.GetString("cp14-ritual-describer-verb-back"),
                Category = VerbCategory.CP14RitualBook,
                Priority = -1,
                Act = () => BackPhase(ent),
            };
            args.Verbs.Add(verb);
        }
    }

    private string GetPhaseDescription(EntityUid uid)
    {
        if (!TryComp<CP14MagicRitualPhaseComponent>(uid, out var phase))
            return string.Empty;

        return GetPhaseDescription((uid, phase));
    }

    private string GetPhaseDescription(Entity<CP14MagicRitualPhaseComponent> ent)
    {
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
            if (edge.Triggers.Count > 0)
            {
                sb.Append($"[bold]{Loc.GetString("cp14-ritual-trigger-header")}[/bold] \n");
                foreach (var trigger in edge.Triggers)
                    sb.Append(trigger.GetGuidebookTriggerDescription(_proto, _entitySystem) + "\n");
            }

            //REQUIREMENTS
            if (edge.Requirements.Count > 0)
            {
                sb.Append($"[bold]{Loc.GetString("cp14-ritual-req-header")}[/bold] \n");
                foreach (var req in edge.Requirements)
                    sb.Append(req.GetGuidebookRequirementDescription(_proto, _entitySystem) + "\n");
            }

            //ACTIONS
            if (edge.Actions.Count > 0)
            {
                sb.Append($"[bold]{Loc.GetString("cp14-ritual-effect-header")}[/bold] \n");
                foreach (var act in edge.Actions)
                    sb.Append(act.GetGuidebookEffectDescription(_proto, _entitySystem) + "\n");
            }
        }
        return sb.ToString();
    }
}
