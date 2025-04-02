using Content.Shared._CP14.Farming.Components;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Shared._CP14.Farming;

public abstract partial class CP14SharedFarmingSystem
{
    private void InitializeInteractions()
    {
        SubscribeLocalEvent<CP14SeedComponent, AfterInteractEvent>(OnSeedInteract);
        SubscribeLocalEvent<CP14PlantGatherableComponent, ActivateInWorldEvent>(OnActivate);
        SubscribeLocalEvent<CP14PlantGatherableComponent, AttackedEvent>(OnAttacked);

        SubscribeLocalEvent<CP14SoilComponent, PlantSeedDoAfterEvent>(OnSeedPlantedDoAfter);
    }

    private void OnAttacked(Entity<CP14PlantGatherableComponent> gatherable, ref AttackedEvent args)
    {
        if (_whitelist.IsWhitelistFailOrNull(gatherable.Comp.ToolWhitelist, args.Used))
            return;

        TryHarvestPlant(gatherable, out _);
    }

    private void OnActivate(Entity<CP14PlantGatherableComponent> gatherable, ref ActivateInWorldEvent args)
    {
        if (args.Handled || !args.Complex)
            return;

        if (_whitelist.IsWhitelistFailOrNull(gatherable.Comp.ToolWhitelist, args.User))
            return;

        TryHarvestPlant(gatherable, out _);
        args.Handled = true;
    }

    public bool TryHarvestPlant(Entity<CP14PlantGatherableComponent> gatheredPlant, out HashSet<EntityUid> result, EntityUid? gatherer = null)
    {
        result = new();

        if (!TryComp<CP14PlantComponent>(gatheredPlant, out var plant))
            return false;

        if (plant.GrowthLevel < gatheredPlant.Comp.GrowthLevelToHarvest)
            return false;

        //if (TryComp<SoundOnGatherComponent>(gatheredPlant, out var soundComp))
        //{
        //    _audio.PlayPvs(soundComp.Sound, Transform(gatheredPlant).Coordinates);
        //}

        if (gatheredPlant.Comp.Loot == null)
            return false;

        var pos = _transform.GetMapCoordinates(gatheredPlant);

        if (_net.IsServer)
        {
            foreach (var (tag, table) in gatheredPlant.Comp.Loot)
            {
                if (tag != "All")
                {
                    if (gatherer != null && !_tag.HasTag(gatherer.Value, tag))
                        continue;
                }

                if (!_proto.TryIndex(table, out var getLoot))
                    continue;

                var spawnLoot = getLoot.GetSpawns(_random);
                foreach (var loot in spawnLoot)
                {
                    var spawnPos = pos.Offset(_random.NextVector2(gatheredPlant.Comp.GatherOffset));
                    result.Add(Spawn(loot, spawnPos));
                }
            }
        }

        if (gatheredPlant.Comp.DeleteAfterHarvest)
            _destructible.DestroyEntity(gatheredPlant);
        else
            AffectGrowth((gatheredPlant, plant), -gatheredPlant.Comp.GrowthCostHarvest);

        return true;
    }

    private void OnSeedInteract(Entity<CP14SeedComponent> seed, ref AfterInteractEvent args)
    {
        if (args.Handled)
            return;

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
        _doAfter.TryStartDoAfter(doAfterArgs);

        args.Handled = true;
    }

    public bool TryPlantSeed(EntityUid seed, EntityUid soil, EntityUid? user)
    {
        if (!TryComp<CP14SoilComponent>(soil, out var soilComp))
            return false;

        if (!TryComp<CP14SeedComponent>(seed, out var seedComp))
            return false;

        if (Exists(soilComp.PlantUid))
        {
            if (user is not null)
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
}
