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

        SubscribeLocalEvent<CP14PlantComponent, EntityUnpausedEvent>(OnUnpaused);
        SubscribeLocalEvent<CP14PlantComponent, MapInitEvent>(OnRegenerationMapInit);

        SubscribeLocalEvent<CP14PlantEnergyFromLightComponent, CP14PlantUpdateEvent>(TakeEnergyFromLight);
        SubscribeLocalEvent<CP14PlantGrowingComponent, CP14PlantUpdateEvent>(PlantGrowing);
    }

    private void PlantGrowing(Entity<CP14PlantGrowingComponent> growing, ref CP14PlantUpdateEvent args)
    {
        if (args.Plant.Energy < growing.Comp.EnergyCost)
            return;

        if (args.Plant.Resource < growing.Comp.ResourceCost)
            return;

        args.Plant.Energy -= growing.Comp.EnergyCost;
        args.Plant.Resource -= growing.Comp.ResourceCost;

        args.Plant.GrowthLevel = MathHelper.Clamp01(args.Plant.GrowthLevel + growing.Comp.GrowthPerUpdate);
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

            var ev = new CP14PlantUpdateEvent(plant);
            RaiseLocalEvent(uid, ev);

            Dirty(new Entity<CP14PlantComponent>(uid, plant));
        }
    }

    private void OnUnpaused(Entity<CP14PlantComponent> ent, ref EntityUnpausedEvent args)
    {
        ent.Comp.NextUpdateTime += args.PausedTime;
    }

    private void OnRegenerationMapInit(Entity<CP14PlantComponent> plant, ref MapInitEvent args)
    {
        var newTime = TimeSpan.FromSeconds(_random.NextFloat(plant.Comp.UpdateFrequency));
        plant.Comp.NextUpdateTime = _timing.CurTime + newTime;
    }

    private void TakeEnergyFromLight(Entity<CP14PlantEnergyFromLightComponent> regeneration, ref CP14PlantUpdateEvent args)
    {
        var gainEnergy = false;
        var daylight = _dayCycle.TryDaylightThere(regeneration, true);

        if (regeneration.Comp.Daylight && daylight)
            gainEnergy = true;

        if (regeneration.Comp.Dark && !daylight)
            gainEnergy = true;

        if (gainEnergy)
            args.Plant.Energy += regeneration.Comp.Energy;
    }
}
