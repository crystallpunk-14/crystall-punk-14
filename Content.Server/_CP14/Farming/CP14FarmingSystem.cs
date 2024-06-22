using Content.Server._CP14.Farming.Components;
using Content.Shared._CP14.DayCycle;
using Content.Shared._CP14.Farming;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
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

        SubscribeLocalEvent<CP14SeedComponent, AfterInteractEvent>(OnSeedInteract);
        SubscribeLocalEvent<CP14PlantRemoverComponent, AfterInteractEvent>(OnPlantRemoverInteract);

        SubscribeLocalEvent<CP14SoilComponent, PlantSeedDoAfterEvent>(OnSeedPlantedDoAfter);
        SubscribeLocalEvent<CP14SoilComponent, PlantRemoveDoAfterEvent>(OnSoilRemoveDoAfter);
        SubscribeLocalEvent<CP14PlantComponent, PlantRemoveDoAfterEvent>(OnPlantRemoveDoAfter);

        SubscribeLocalEvent<CP14PlantComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<CP14PlantEnergyFromLightComponent, MapInitEvent>(OnRegenerationMapInit);
    }

    private void OnRegenerationMapInit(Entity<CP14PlantEnergyFromLightComponent> regeneration, ref MapInitEvent args)
    {
        regeneration.Comp.NextUpdateTime = _timing.CurTime + TimeSpan.FromSeconds(_random.NextFloat(regeneration.Comp.MinUpdateFrequency, regeneration.Comp.MaxUpdateFrequency));
    }


    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<CP14PlantComponent, CP14PlantEnergyFromLightComponent>();
        while (query.MoveNext(out var uid, out var plant, out var regeneration))
        {
            if (_timing.CurTime <= regeneration.NextUpdateTime)
                continue;

            regeneration.NextUpdateTime = _timing.CurTime + TimeSpan.FromSeconds(_random.NextFloat(regeneration.MinUpdateFrequency, regeneration.MaxUpdateFrequency));

            var gainEnergy = false;
            var daylight = _dayCycle.TryDaylightThere(uid, true);

            if (regeneration.Daylight && daylight)
                gainEnergy = true;

            if (regeneration.Dark && !daylight)
                gainEnergy = true;

            if (gainEnergy)
                plant.Energy += regeneration.Energy;
        }
    }

    private void OnSeedInteract(Entity<CP14SeedComponent> seed, ref AfterInteractEvent args)
    {
        if (!TryComp<CP14SoilComponent>(args.Target, out var soil))
            return;

        if (EntityManager.EntityExists(soil.PlantUid))
        {
            _popup.PopupEntity(Loc.GetString("cp14-farming-soil-interact-plant-exist"), args.Target.Value, args.User);
            return;
        }
        var doAfterArgs =
            new DoAfterArgs(EntityManager, args.User, seed.Comp.PlantingTime, new PlantSeedDoAfterEvent(), args.Target, args.Used, args.Target)
            {
                BreakOnDamage = true,
                BlockDuplicate = true,
                BreakOnMove = true,
                BreakOnHandChange = true,
            };
        _doAfterSystem.TryStartDoAfter(doAfterArgs);
    }

    private void OnPlantRemoverInteract(Entity<CP14PlantRemoverComponent> ent, ref AfterInteractEvent args)
    {
        if (!HasComp<CP14PlantComponent>(args.Target) &&
            !HasComp<CP14SoilComponent>(args.Target))
            return;

        _audio.PlayPvs(ent.Comp.Sound, ent);

        var doAfterArgs =
            new DoAfterArgs(EntityManager, args.User, ent.Comp.DoAfter, new PlantRemoveDoAfterEvent(), args.Target)
            {
                BreakOnDamage = true,
                BlockDuplicate = true,
                BreakOnMove = true,
                BreakOnHandChange = true,
            };
        _doAfterSystem.TryStartDoAfter(doAfterArgs);
    }

    private void OnSeedPlantedDoAfter(Entity<CP14SoilComponent> soil, ref PlantSeedDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || args.Args.Used == null || args.Target == null)
            return;

        if (!TryPlantSeed(args.Target.Value, soil, args.User))
            return;

        //Audio
        QueueDel(args.Target); //delete seed

        args.Handled = true;
    }

    private void OnPlantRemoveDoAfter(Entity<CP14PlantComponent> ent, ref PlantRemoveDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        QueueDel(ent); //TODO harvers + remove

        args.Handled = true;
    }

    private void OnSoilRemoveDoAfter(Entity<CP14SoilComponent> ent, ref PlantRemoveDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        QueueDel(ent); //TODO harvers + remove

        args.Handled = true;
    }

    public bool TryPlantSeed(EntityUid seed, EntityUid soil, EntityUid? user)
    {
        if (!TryComp<CP14SoilComponent>(soil, out var soilComp))
            return false;

        if (!TryComp<CP14SeedComponent>(seed, out var seedComp))
            return false;

        if (EntityManager.EntityExists(soilComp.PlantUid))
        {
            if (user != null)
                _popup.PopupEntity(Loc.GetString("cp14-farming-soil-interact-plant-exist"), soil, user.Value);

            return false;
        }

        var plant = SpawnAttachedTo(seedComp.PlantProto, Transform(soil).Coordinates);

        if (!TryComp<CP14PlantComponent>(plant, out var plantComp))
            return false;

        _transform.SetParent(plant, soil);
        soilComp.PlantUid = plant;
        plantComp.SoilUid = soil;

        return true;
    }

    private void OnMapInit(Entity<CP14PlantComponent> plant, ref MapInitEvent args)
    {

    }
}
