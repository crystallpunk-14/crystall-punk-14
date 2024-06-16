using Content.Server._CP14.Farming.Components;
using Content.Shared._CP14.DayCycle;
using Content.Shared._CP14.Farming;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._CP14.Farming;

public sealed partial class CP14FarmingSystem : CP14SharedFarmingSystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly CP14DayCycleSystem _dayCycle = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14SoilComponent, InteractUsingEvent>(OnSoilInteract);
        SubscribeLocalEvent<CP14SoilComponent, PlantSeedDoAfterEvent>(OnSeedPlantedDoAfter);

        SubscribeLocalEvent<CP14PlantComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<CP14PlantEnergyRegenerationComponent, MapInitEvent>(OnRegenerationMapInit);
    }

    private void OnRegenerationMapInit(Entity<CP14PlantEnergyRegenerationComponent> regeneration, ref MapInitEvent args)
    {
        regeneration.Comp.NextUpdateTime = _timing.CurTime + TimeSpan.FromSeconds(_random.NextFloat(regeneration.Comp.MinUpdateFrequency, regeneration.Comp.MaxUpdateFrequency));
    }


    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<CP14PlantComponent, CP14PlantEnergyRegenerationComponent>();
        while (query.MoveNext(out var uid, out var plant, out var regeneration))
        {
            if (_timing.CurTime <= regeneration.NextUpdateTime)
                continue;

            regeneration.NextUpdateTime = _timing.CurTime + TimeSpan.FromSeconds(_random.NextFloat(regeneration.MinUpdateFrequency, regeneration.MaxUpdateFrequency));

            var gainEnergy = false;
            var daylight = _dayCycle.TryDaylightThere(uid, true);

            if (regeneration.OnDaylight && daylight)
                gainEnergy = true;

            if (regeneration.InDark && !daylight)
                gainEnergy = true;

            if (gainEnergy)
                plant.Energy += regeneration.Energy;
        }
    }

    private void OnSoilInteract(Entity<CP14SoilComponent> soil, ref InteractUsingEvent args)
    {
        if (!TryComp<CP14SeedComponent>(args.Used, out var seed))
            return;

        //Audio
        //Popup

        var doAfterArgs =
            new DoAfterArgs(EntityManager, args.User, seed.PlantingTime, new PlantSeedDoAfterEvent(), soil, args.Used, soil)
            {
                BreakOnDamage = true,
                BlockDuplicate = true,
                BreakOnMove = true,
                BreakOnHandChange = true
            };
        _doAfterSystem.TryStartDoAfter(doAfterArgs);
    }

    private void OnSeedPlantedDoAfter(Entity<CP14SoilComponent> ent, ref PlantSeedDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || args.Args.Used == null)
            return;

        if (!TryComp<CP14SeedComponent>(args.Target, out var seed))
            return;

        var plant = SpawnAttachedTo(seed.PlantProto, Transform(ent).Coordinates);

        if (TryComp<CP14PlantComponent>(plant, out var plantComp))
        {
            TryRootPlant(plantComp, ent);
        }
        //Audio
        QueueDel(args.Target); //delete seed
    }

    private bool TryRootPlant(CP14PlantComponent component, Entity<CP14SoilComponent> soilUid)
    {
        //Parenting to soil here?

        component.SoilUid = soilUid;
        return true;
    }

    private void OnMapInit(Entity<CP14PlantComponent> plant, ref MapInitEvent args)
    {

    }

}
