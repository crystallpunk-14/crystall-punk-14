using Content.Shared._CP14.MagicRitual;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._CP14.MagicRituals;

public partial class CP14RitualSystem : CP14SharedRitualSystem
{
    [Dependency] private readonly IEntitySystemManager _entitySystem = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly PointLightSystem _pointLight = default!;

    public override void Initialize()
    {
        base.Initialize();

        InitializeTriggers();
        InitializeDescriber();

        SubscribeLocalEvent<CP14MagicRitualComponent, MapInitEvent>(OnRitualInit);
        SubscribeLocalEvent<CP14MagicRitualPhaseComponent, CP14RitualPhaseBoundEvent>(OnPhaseBound);
        SubscribeLocalEvent<CP14MagicRitualPhaseComponent, CP14RitualTriggerEvent>(OnPhaseTrigger);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        UpdateTriggers(frameTime);
    }

    public void StartRitual(Entity<CP14MagicRitualComponent> ritual)
    {
        EndRitual(ritual);

        var ev = new CP14RitualStartEvent(ritual);
        RaiseLocalEvent(ritual, ev);

        ChangePhase(ritual, ritual.Comp.StartPhase);
    }

    private void ChangePhase(Entity<CP14MagicRitualComponent> ritual, EntProtoId newPhase)
    {
        QueueDel(ritual.Comp.CurrentPhase);

        var newPhaseEnt = Spawn(newPhase, Transform(ritual).Coordinates);
        _transform.SetParent(newPhaseEnt, ritual);
        var newPhaseComp = EnsureComp<CP14MagicRitualPhaseComponent>(newPhaseEnt);

        ritual.Comp.CurrentPhase = (newPhaseEnt, newPhaseComp);
        newPhaseComp.Ritual = ritual;

        var ev = new CP14RitualPhaseBoundEvent(ritual, newPhaseEnt);
        RaiseLocalEvent(ritual, ev);
        RaiseLocalEvent(newPhaseEnt, ev);
    }

    public void EndRitual(Entity<CP14MagicRitualComponent> ritual)
    {
        if (ritual.Comp.CurrentPhase is null)
            return;

        QueueDel(ritual.Comp.CurrentPhase);
        ritual.Comp.CurrentPhase = null;

        var ev = new CP14RitualEndEvent(ritual);
        RaiseLocalEvent(ritual, ev);
    }

    private void OnPhaseBound(Entity<CP14MagicRitualPhaseComponent> ent, ref CP14RitualPhaseBoundEvent args)
    {
        if (!TryComp<CP14MagicRitualComponent>(args.Ritual, out var ritual))
            return;

        _pointLight.SetColor(ent, ent.Comp.PhaseColor);
    }

    private void OnRitualInit(Entity<CP14MagicRitualComponent> ritual, ref MapInitEvent args)
    {
        StartRitual(ritual);
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
