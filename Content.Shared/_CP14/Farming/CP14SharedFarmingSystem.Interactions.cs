using System.Linq;
using Content.Shared._CP14.Farming.Components;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Maps;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Timing;

namespace Content.Shared._CP14.Farming;

public abstract partial class CP14SharedFarmingSystem
{
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private void InitializeInteractions()
    {
        SubscribeLocalEvent<CP14SeedComponent, AfterInteractEvent>(OnSeedInteract);
        SubscribeLocalEvent<CP14PlantGatherableComponent, ActivateInWorldEvent>(OnActivate);
        SubscribeLocalEvent<CP14PlantGatherableComponent, AttackedEvent>(OnAttacked);

        SubscribeLocalEvent<CP14SeedComponent, PlantSeedDoAfterEvent>(OnSeedPlantedDoAfter);
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

    public bool TryHarvestPlant(Entity<CP14PlantGatherableComponent> gatheredPlant,
        out HashSet<EntityUid> result,
        EntityUid? gatherer = null)
    {
        result = new();

        if (!PlantQuery.TryComp(gatheredPlant, out var plant))
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

        if (!CanPlantSeed(seed, args.ClickLocation, args.User))
            return;

        var doAfterArgs =
            new DoAfterArgs(EntityManager,
                args.User,
                seed.Comp.PlantingTime,
                new PlantSeedDoAfterEvent(GetNetCoordinates(args.ClickLocation)),
                seed)
            {
                BreakOnDamage = true,
                BlockDuplicate = false,
                BreakOnMove = true,
                BreakOnHandChange = true,
            };
        _doAfter.TryStartDoAfter(doAfterArgs);

        args.Handled = true;
    }

    private void OnSeedPlantedDoAfter(Entity<CP14SeedComponent> ent, ref PlantSeedDoAfterEvent args)
    {
        if (_net.IsClient || args.Handled || args.Cancelled)
            return;

        var position = GetCoordinates(args.Coordinates);
        if (!CanPlantSeed(ent, position, args.User))
            return;

        args.Handled = true;

        Spawn(ent.Comp.PlantProto, position);
        QueueDel(ent);
    }

    public bool CanPlantSeed(Entity<CP14SeedComponent> seed, EntityCoordinates position, EntityUid? user)
    {
        var map = _transform.GetMap(position);
        if (!TryComp<MapGridComponent>(map, out var gridComp))
            return false;

        var tileRef = position.GetTileRef();

        if (tileRef is null)
            return false;

        var tile = tileRef.Value.Tile.GetContentTileDefinition();

        if (!seed.Comp.SoilTile.Contains(tile))
        {
            if (user is not null && _timing.IsFirstTimePredicted && _net.IsClient)
            {
                _popup.PopupEntity(Loc.GetString("cp14-farming-soil-wrong", ("seed", MetaData(seed).EntityName)),
                    user.Value,
                    user.Value);
            }

            return false;
        }

        if (_map.GetAnchoredEntities((map.Value, gridComp), position).ToList().Count > 0)
        {
            if (user is not null)
                _popup.PopupEntity(Loc.GetString("cp14-farming-soil-occupied"), user.Value, user.Value);
            return false;
        }

        return true;
    }
}
