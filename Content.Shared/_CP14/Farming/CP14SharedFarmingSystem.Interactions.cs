using System.Linq;
using Content.Shared._CP14.Farming.Components;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Maps;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Timing;

namespace Content.Shared._CP14.Farming;

public abstract partial class CP14SharedFarmingSystem
{
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    private void InitializeInteractions()
    {
        SubscribeLocalEvent<CP14SeedComponent, AfterInteractEvent>(OnSeedInteract);
        SubscribeLocalEvent<CP14PlantGatherableComponent, InteractUsingEvent>(OnActivate);
        SubscribeLocalEvent<CP14PlantGatherableComponent, CP14PlantGatherDoAfterEvent>(OnGatherDoAfter);

        SubscribeLocalEvent<CP14SeedComponent, CP14PlantSeedDoAfterEvent>(OnSeedPlantedDoAfter);
    }

    private void OnActivate(Entity<CP14PlantGatherableComponent> gatherable, ref InteractUsingEvent args)
    {
        if (args.Handled)
            return;

        if (_whitelist.IsWhitelistFailOrNull(gatherable.Comp.ToolWhitelist, args.Used))
            return;

        TryHarvestPlant(gatherable, args.Used, args.User);
        args.Handled = true;
    }

    private bool CanHarvestPlant(Entity<CP14PlantGatherableComponent> gatherable)
    {
        if (PlantQuery.TryComp(gatherable, out var plant))
        {
            if (plant.GrowthLevel < gatherable.Comp.GrowthLevelToHarvest)
                return false;
        }

        if (gatherable.Comp.Loot == null)
            return false;

        return true;
    }

    private bool TryHarvestPlant(Entity<CP14PlantGatherableComponent> gatherable,
        EntityUid used,
        EntityUid user)
    {
        if (!CanHarvestPlant(gatherable))
            return false;

        _audio.PlayPvs(gatherable.Comp.GatherSound, Transform(gatherable).Coordinates);

        var doAfterArgs =
            new DoAfterArgs(EntityManager,
                user,
                gatherable.Comp.GatherDelay,
                new CP14PlantGatherDoAfterEvent(),
                gatherable,
                used: used)
            {
                BreakOnDamage = true,
                BlockDuplicate = false,
                BreakOnMove = true,
                BreakOnHandChange = true,
            };

        _doAfter.TryStartDoAfter(doAfterArgs);

        return true;
    }

    private void OnGatherDoAfter(Entity<CP14PlantGatherableComponent> gatherable, ref CP14PlantGatherDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        if (!CanHarvestPlant(gatherable))
            return;

        args.Handled = true;

        HarvestPlant(gatherable, out _, args.Used);
    }

    public void HarvestPlant(Entity<CP14PlantGatherableComponent> gatherable,
        out HashSet<EntityUid> result,
        EntityUid? used)
    {
        result = new();

        if (_net.IsClient)
            return;

        if (gatherable.Comp.Loot == null)
            return;

        var pos = _transform.GetMapCoordinates(gatherable);

        foreach (var (tag, table) in gatherable.Comp.Loot)
        {
            if (tag != "All")
            {
                if (used != null && !_tag.HasTag(used.Value, tag))
                    continue;
            }

            if (!_proto.TryIndex(table, out var getLoot))
                continue;

            var spawnLoot = getLoot.GetSpawns(_random);
            foreach (var loot in spawnLoot)
            {
                var spawnPos = pos.Offset(_random.NextVector2(gatherable.Comp.GatherOffset));
                result.Add(Spawn(loot, spawnPos));
            }
        }

        _audio.PlayPvs(gatherable.Comp.GatherSound, Transform(gatherable).Coordinates);

        if (gatherable.Comp.DeleteAfterHarvest)
            _destructible.DestroyEntity(gatherable);
        else
        {
            if (PlantQuery.TryComp(gatherable, out var plant))
            {
                AffectGrowth((gatherable, plant), -gatherable.Comp.GrowthCostHarvest);
            }
        }
    }

    private void OnSeedInteract(Entity<CP14SeedComponent> seed, ref AfterInteractEvent args)
    {
        if (args.Handled || !args.CanReach)
            return;

        if (!CanPlantSeed(seed, args.ClickLocation, args.User))
            return;

        var doAfterArgs =
            new DoAfterArgs(EntityManager,
                args.User,
                seed.Comp.PlantingTime,
                new CP14PlantSeedDoAfterEvent(GetNetCoordinates(args.ClickLocation)),
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

    private void OnSeedPlantedDoAfter(Entity<CP14SeedComponent> ent, ref CP14PlantSeedDoAfterEvent args)
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

        foreach (var anchored in _map.GetAnchoredEntities((map.Value, gridComp), position))
        {
            if (PlantQuery.TryComp(anchored, out var plant))
            {
                if (user is not null && _timing.IsFirstTimePredicted && _net.IsClient)
                    _popup.PopupEntity(Loc.GetString("cp14-farming-soil-occupied"), user.Value, user.Value);
                return false;
            }
        }

        return true;
    }
}
