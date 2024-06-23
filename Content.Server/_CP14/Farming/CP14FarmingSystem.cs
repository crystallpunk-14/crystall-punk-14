using Content.Server._CP14.Farming.Components;
using Content.Shared._CP14.DayCycle;
using Content.Shared._CP14.Farming;
using Content.Shared.DoAfter;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._CP14.Farming;

public sealed partial class CP14FarmingSystem : CP14SharedFarmingSystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly CP14DayCycleSystem _dayCycle = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        InitializeTools();
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

            var newTime = TimeSpan.FromSeconds(plant.UpdateFrequency);
            plant.NextUpdateTime = _timing.CurTime + newTime;

            var energyEv = new CP14PlantEnergyUpdateEvent();
            RaiseLocalEvent(uid, energyEv);
            if (!energyEv.Canceled)
                plant.Energy += energyEv.Energy;

            var resourceEv = new CP14PlantResourceUpdateEvent();
            RaiseLocalEvent(uid, resourceEv);
            if (!resourceEv.Canceled)
                plant.Resource += resourceEv.Resource;

            var ev = new CP14AfterPlantUpdateEvent(plant);
            RaiseLocalEvent(uid, ev);

            Dirty(new Entity<CP14PlantComponent>(uid, plant));
        }
    }

    private void OnUnpaused(Entity<CP14PlantComponent> ent, ref EntityUnpausedEvent args)
    {
        ent.Comp.NextUpdateTime += args.PausedTime;
    }

    private void OnMapInit(Entity<CP14PlantComponent> plant, ref MapInitEvent args)
    {
        var newTime = TimeSpan.FromSeconds(_random.NextFloat(plant.Comp.UpdateFrequency));
        plant.Comp.NextUpdateTime = _timing.CurTime + newTime;
    }
}
