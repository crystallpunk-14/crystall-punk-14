using Content.Server._CP14.StationDungeonMap.Components;
using Content.Server.GameTicking.Events;
using Content.Server.Station.Components;
using Content.Server.Station.Events;
using Content.Server.Station.Systems;
using Content.Shared.Maps;
using Content.Shared.Station.Components;
using Content.Shared.Teleportation.Systems;
using Robust.Server.GameObjects;
using Robust.Server.Maps;
using Robust.Shared.Map;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Server._CP14.StationDungeonMap.EntitySystems;

public sealed partial class CP14StationZLevelsSystem : EntitySystem
{
    [Dependency] private readonly MapSystem _map = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly MapLoaderSystem _mapLoader = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly TileSystem _tile = default!;
    [Dependency] private readonly LinkedEntitySystem _linkedEntity = default!;

    public override void Initialize()
    {
        base.Initialize();
        AutoPortalInitialize();

        SubscribeLocalEvent<RoundStartingEvent>(OnRoundStart);
        SubscribeLocalEvent<CP14StationZLevelsComponent, StationPostInitEvent>(OnStationPostInit);
    }

    private void OnRoundStart(RoundStartingEvent ev)
    {
        var query = EntityQueryEnumerator<CP14ZLevelAutoPortalComponent>();
        while (query.MoveNext(out var uid, out var portal))
        {
            InitPortal((uid, portal));
        }
    }

    private void OnStationPostInit(Entity<CP14StationZLevelsComponent> ent, ref StationPostInitEvent args)
    {
        if (ent.Comp.Initialized)
            return;

        if (!TryComp(ent, out StationDataComponent? dataComp))
        {
            Log.Error($"Failed to init CP14StationZLevelsSystem: no StationData");
            return;
        }

        var defaultMap = _station.GetLargestGrid(dataComp);

        if (defaultMap is null)
        {
            Log.Error($"Failed to init CP14StationZLevelsSystem: defaultMap is null");
            return;
        }

        ent.Comp.LevelEntities.Add(Transform(defaultMap.Value).MapID, ent.Comp.DefaultMapLevel);

        ent.Comp.Initialized = true;

        foreach (var (zLevel, level) in ent.Comp.Levels)
        {
            var path = level.Path.ToString();
            if (level.Path is null)
            {
                Log.Error($"path {path} for CP14StationZLevelsSystem at level {zLevel} don't exist!");
                continue;
            }

            SetZLevel(ent, zLevel, level.Path.Value);
        }
    }

    public MapId? SetZLevel(Entity<CP14StationZLevelsComponent> ent, int zLevel, ResPath resPath, bool force = false)
    {
        if (ent.Comp.LevelEntities.ContainsValue(zLevel))
        {
            if (!force)
            {
                Log.Error($"Key duplication for CP14StationZLevelsSystem at level {zLevel}!");
                return null;
            }

            foreach (var (key, value) in ent.Comp.LevelEntities)
            {
                if (value == zLevel && _map.MapExists(key))
                {
                    ent.Comp.LevelEntities.Remove(key);
                    Del(_map.GetMap(key));
                }
            }
        }

        var mapUid = _map.CreateMap(out var mapId);
        var member = EnsureComp<StationMemberComponent>(mapUid);
        member.Station = ent;

        Log.Info($"Created map {mapId} for CP14StationZLevelsSystem at level {zLevel}");

        var options = new MapLoadOptions { LoadMap = true };

        if (!_mapLoader.TryLoad(mapId, resPath.ToString(), out var grids, options))
        {
            Log.Error($"Failed to load map for CP14StationZLevelsSystem at level {zLevel}!");
            Del(mapUid);
            return null;
        }
        ent.Comp.LevelEntities.TryAdd(mapId, zLevel);

        return mapId;
    }

    public MapId? GetMapOffset(EntityUid mapUid, int offset)
    {
        var query = EntityQueryEnumerator<CP14StationZLevelsComponent, StationDataComponent>();
        while (query.MoveNext(out var uid, out var zLevel, out _))
        {
            if (!zLevel.LevelEntities.TryGetValue(Transform(mapUid).MapID, out var currentLevel))
                continue;

            var targetLevel = currentLevel + offset;

            if (!zLevel.LevelEntities.ContainsValue(targetLevel))
                continue;

            foreach (var (key, value) in zLevel.LevelEntities)
            {
                if (value == targetLevel && _map.MapExists(key))
                    return key;
            }
        }
        return null;
    }
}
