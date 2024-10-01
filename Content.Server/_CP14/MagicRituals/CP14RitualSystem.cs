using System.Text;
using Content.Server._CP14.MagicRituals.Components;
using Content.Shared._CP14.MagicRitual;
using Content.Shared.Examine;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._CP14.MagicRituals;

public partial class CP14RitualSystem : EntitySystem
{
    [Dependency] private readonly IEntitySystemManager _entitySystem = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly TransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        InitializeTriggers();

        SubscribeLocalEvent<CP14MagicRitualComponent, MapInitEvent>(OnRitualInit);
        SubscribeLocalEvent<CP14MagicRitualComponent, ExaminedEvent>(OnRitualExamine);
        SubscribeLocalEvent<CP14MagicRitualPhaseComponent, CP14RitualTriggerEvent>(OnPhaseTrigger);
    }

    private void OnRitualExamine(Entity<CP14MagicRitualComponent> ent, ref ExaminedEvent args)
    {
        //TEMP TODO
        if (ent.Comp.CurrentPhase is null)
            return;

        var sb = new StringBuilder();

        foreach (var edge in ent.Comp.CurrentPhase.Value.Comp.Edges)
        {
            if (!_proto.TryIndex(edge.Target, out var targetIndexed))
                continue;

            sb.Append(Loc.GetString("cp14-ritual-effect-header", ("node", targetIndexed.Name)));
            sb.Append("\n");

            foreach (var act in edge.Actions)
            {
                sb.Append(act.GetGuidebookEffectDescription(_proto, _entitySystem));
            }
        }
        args.PushMarkup(sb.ToString());
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        UpdateTriggers(frameTime);
    }

    public void ChangeRitualStability(Entity<CP14MagicRitualComponent> ritual, float dStab)
    {
        var newS = MathHelper.Clamp01(ritual.Comp.Stability + dStab);

        var ev = new CP14RitualStabilityChangedEvent(ritual.Comp.Stability, newS);
        RaiseLocalEvent(ritual, ev);

        ritual.Comp.Stability = newS;
    }

    private void OnRitualInit(Entity<CP14MagicRitualComponent> ritual, ref MapInitEvent args)
    {
        ChangePhase(ritual, ritual.Comp.StartPhase);
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

    private void ChangePhase(Entity<CP14MagicRitualComponent> ritual, EntProtoId newPhase)
    {
        QueueDel(ritual.Comp.CurrentPhase);

        var newPhaseEnt = Spawn(newPhase, Transform(ritual).Coordinates);
        _transform.SetParent(newPhaseEnt, ritual);
        var newPhaseComp = EnsureComp<CP14MagicRitualPhaseComponent>(newPhaseEnt);

        ritual.Comp.CurrentPhase = (newPhaseEnt, newPhaseComp);
        newPhaseComp.Ritual = ritual;
    }
}
