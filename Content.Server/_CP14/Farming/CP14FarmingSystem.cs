using Content.Server.DoAfter;
using Content.Server.Popups;
using Content.Shared._CP14.DayCycle;
using Content.Shared._CP14.Farming;
using Content.Shared.Chemistry.EntitySystems;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._CP14.Farming;

public sealed partial class CP14FarmingSystem : CP14SharedFarmingSystem
{
    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    [Dependency] private readonly CP14DayCycleSystem _dayCycle = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;

    public override void Initialize()
    {
        base.Initialize();

        InitializeInteractions();
        InitializeResources();

        SubscribeLocalEvent<CP14PlantComponent, EntityUnpausedEvent>(OnUnpaused);
        SubscribeLocalEvent<CP14PlantComponent, MapInitEvent>(OnMapInit);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<CP14PlantComponent>();
        while (query.MoveNext(out var uid, out var plant))
        {
            if (_timing.CurTime <= plant.NextUpdateTime)
                continue;

            var newTime = plant.UpdateFrequency;
            plant.NextUpdateTime = _timing.CurTime + newTime;

            var ev = new CP14PlantUpdateEvent((uid, plant));
            RaiseLocalEvent(uid, ev);

            plant.Resource += ev.ResourceDelta;
            plant.Energy += ev.EnergyDelta;

            var ev2 = new CP14AfterPlantUpdateEvent((uid, plant));
            RaiseLocalEvent(uid, ev2);

            Dirty(uid, plant);
        }
    }

    private void OnUnpaused(Entity<CP14PlantComponent> ent, ref EntityUnpausedEvent args)
    {
        ent.Comp.NextUpdateTime += args.PausedTime;
    }

    private void OnMapInit(Entity<CP14PlantComponent> plant, ref MapInitEvent args)
    {
        var newTime = plant.Comp.UpdateFrequency;
        plant.Comp.NextUpdateTime = _timing.CurTime + newTime;
    }
}
