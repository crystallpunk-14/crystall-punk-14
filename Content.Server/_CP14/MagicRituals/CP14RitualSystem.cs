using Content.Server._CP14.MagicRituals.Components;
using Content.Server.Stack;
using Content.Shared._CP14.MagicRitual;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._CP14.MagicRituals;

public partial class CP14RitualSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly StackSystem _stack = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly TransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        InitializeTriggers();
        InitializeRequirements();

        SubscribeLocalEvent<CP14MagicRitualComponent, MapInitEvent>(OnRitualInit);
        SubscribeLocalEvent<CP14MagicRitualPhaseComponent, CP14RitualTriggerEvent>(OnPhaseTriggerAttempt);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        UpdateTriggers(frameTime);
    }

    private void OnRitualInit(Entity<CP14MagicRitualComponent> ritual, ref MapInitEvent args)
    {
        ChangePhase(ritual, ritual.Comp.StartPhase);
    }

    private void OnPhaseTriggerAttempt(Entity<CP14MagicRitualPhaseComponent> phase, ref CP14RitualTriggerEvent args)
    {
        if (args.NextPhase is null || phase.Comp.Ritual is null)
            return;

        ChangePhase(phase.Comp.Ritual.Value, args.NextPhase.Value);
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
