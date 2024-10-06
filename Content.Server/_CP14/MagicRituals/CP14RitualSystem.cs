using System.Text;
using Content.Server.Speech.Components;
using Content.Shared._CP14.MagicRitual;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Verbs;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._CP14.MagicRituals;

public partial class CP14RitualSystem : CP14SharedRitualSystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly IEntitySystemManager _entitySystem = default!;
    [Dependency] private readonly PointLightSystem _pointLight = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly TransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        InitializeDescriber();
        InitializeVisuals();

        SubscribeLocalEvent<CP14MagicRitualComponent, CP14ActivateRitualDoAfter>(OnActivateRitual);
        SubscribeLocalEvent<CP14MagicRitualComponent, GetVerbsEvent<AlternativeVerb>>(OnAlternativeVerb);

        SubscribeLocalEvent<CP14MagicRitualPhaseComponent, CP14RitualTriggerEvent>(OnPhaseTrigger);

        SubscribeLocalEvent<CP14MagicRitualOrbComponent, ExaminedEvent>(OnOrbExamine);
    }

    private void OnActivateRitual(Entity<CP14MagicRitualComponent> ent, ref CP14ActivateRitualDoAfter args)
    {
        if (args.Cancelled || args.Handled)
            return;

        args.Handled = true;

        StartRitual(ent);
    }

    private void OnAlternativeVerb(Entity<CP14MagicRitualComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanInteract || ent.Comp.CurrentPhase is not null)
            return;

        var user = args.User;
        AlternativeVerb verb = new()
        {
            Act = () =>
            {
                var doAfterArgs =
                    new DoAfterArgs(EntityManager, user, ent.Comp.ActivationTime, new CP14ActivateRitualDoAfter(), ent, ent)
                    {
                        BreakOnDamage = true,
                        BreakOnMove = true,
                    };

                _doAfter.TryStartDoAfter(doAfterArgs);
            },
            Text = Loc.GetString("cp14-ritual-verb-text"),
            Priority = 1,
        };
        args.Verbs.Add(verb);
    }


    private void OnOrbExamine(Entity<CP14MagicRitualOrbComponent> ent, ref ExaminedEvent args)
    {
        var sb = new StringBuilder();
        sb.Append(Loc.GetString("cp14-ritual-orb-examine", ("name", MetaData(ent).EntityName)) + "\n");
        foreach (var orbType in ent.Comp.Powers)
        {
            if (!_proto.TryIndex(orbType.Key, out var indexedType))
                continue;

            sb.Append($"[color={indexedType.Color.ToHex()}]");
            sb.Append(Loc.GetString("cp14-ritual-entry-item",
                ("name", Loc.GetString(indexedType.Name)),
                ("count", orbType.Value)));
            sb.Append($"[/color] \n");
        }

        args.PushMarkup(sb.ToString());
    }

    public void StartRitual(Entity<CP14MagicRitualComponent> ritual)
    {
        EndRitual(ritual);

        var ev = new CP14RitualStartEvent(ritual);
        RaiseLocalEvent(ritual, ev);

        ChangePhase(ritual, ritual.Comp.StartPhase);
        _appearance.SetData(ritual, RitualVisuals.Enabled, true);
    }

    private void ChangePhase(Entity<CP14MagicRitualComponent> ritual, EntProtoId newPhase)
    {
        QueueDel(ritual.Comp.CurrentPhase);

        var newPhaseEnt = Spawn(newPhase, Transform(ritual).Coordinates);
        _transform.SetParent(newPhaseEnt, ritual);
        var newPhaseComp = EnsureComp<CP14MagicRitualPhaseComponent>(newPhaseEnt);

        ritual.Comp.CurrentPhase = (newPhaseEnt, newPhaseComp);
        newPhaseComp.Ritual = ritual;

        foreach (var edge in newPhaseComp.Edges)
        {
            foreach (var trigger in edge.Triggers)
            {
                trigger.Initialize(EntityManager, ritual.Comp.CurrentPhase.Value, edge);
            }
        }

        var ev = new CP14RitualPhaseBoundEvent(ritual, newPhaseEnt);
        RaiseLocalEvent(ritual, ev);
        RaiseLocalEvent(newPhaseEnt, ev);

        if (newPhaseComp.DeadEnd)
            EndRitual(ritual);
    }

    public void EndRitual(Entity<CP14MagicRitualComponent> ritual)
    {
        if (ritual.Comp.CurrentPhase is null)
            return;

        QueueDel(ritual.Comp.CurrentPhase);
        ritual.Comp.CurrentPhase = null;

        var ev = new CP14RitualEndEvent(ritual);
        RaiseLocalEvent(ritual, ev);

        _appearance.SetData(ritual, RitualVisuals.Enabled, false);

        foreach (var orb in ritual.Comp.Orbs)
        {
            QueueDel(orb);
        }
    }

    private void OnPhaseTrigger(Entity<CP14MagicRitualPhaseComponent> phase, ref CP14RitualTriggerEvent args)
    {
        if (phase.Comp.Ritual is null)
            return;

        RitualPhaseEdge? selectedEdge = null;
        foreach (var edge in phase.Comp.Edges)
        {
            if (edge.Target == args.NextPhase)
            {
                selectedEdge = edge;
                break;
            }
        }

        if (selectedEdge is null)
            return;

        var passed = true;
        foreach (var req in selectedEdge.Value.Requirements)
        {
            if (!req.Check(EntityManager, phase, phase.Comp.Ritual.Value.Comp.Stability)) //lol
            {
                ChangeRitualStability(phase.Comp.Ritual.Value, -req.FailStabilityCost);
                passed = false;
                break;
            }
        }

        if (!passed)
            return;

        foreach (var action in selectedEdge.Value.Actions)
        {
            action.Effect(EntityManager, _transform, phase);
        }

        ChangePhase(phase.Comp.Ritual.Value, args.NextPhase);
    }
}
