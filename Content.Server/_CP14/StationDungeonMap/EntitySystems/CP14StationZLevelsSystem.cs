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

namespace Content.Server._CP14.StationDungeonMap.EntitySystems;

public sealed partial class CP14StationZLevelsSystem : EntitySystem
{
    [Dependency] private readonly MapSystem _map = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly MapLoaderSystem _mapLoader = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly ITileDefinitionManager _tileDefManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly TileSystem _tile = default!;
    [Dependency] private readonly SharedMapSystem _maps = default!;
    [Dependency] private readonly LinkedEntitySystem _linkedEntity = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14ZLevelAutoPortalComponent, MapInitEvent>(OnPortalMapInit);
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

        foreach (var (map, level) in ent.Comp.Levels)
        {
            if (ent.Comp.LevelEntities.ContainsValue(map))
            {
                Log.Error($"Key duplication for CP14StationZLevelsSystem at level {map}!");
                continue;
            }

            var path = level.Path.ToString();
            if (path is null)
            {
                Log.Error($"path {path} for CP14StationZLevelsSystem at level {map} don't exist!");
                continue;
            }

            var mapUid = _map.CreateMap(out var mapId);
            var member = EnsureComp<StationMemberComponent>(mapUid);
            member.Station = ent;

            Log.Info($"Created map {mapId} for CP14StationZLevelsSystem at level {map}");
            var options = new MapLoadOptions { LoadMap = true };

            if (!_mapLoader.TryLoad(mapId, path, out var grids, options))
            {
                Log.Error($"Failed to load map for CP14StationZLevelsSystem at level {map}!");
                Del(mapUid);
                continue;
            }
            ent.Comp.LevelEntities.Add(mapId, map);
        }
    }

    private void OnPortalMapInit(Entity<CP14ZLevelAutoPortalComponent> autoPortal, ref MapInitEvent args)
    {
        InitPortal(autoPortal);
    }

    private void InitPortal(Entity<CP14ZLevelAutoPortalComponent> autoPortal)
    {
        var mapId = Transform(autoPortal).MapUid;
        if (mapId is null)
            return;

        var offsetMap = GetMapOffset(mapId.Value, autoPortal.Comp.ZLevelOffset);

        if (offsetMap is null)
            return;

        var currentWorldPos = _transform.GetWorldPosition(autoPortal);
        var targetMapPos = new MapCoordinates(currentWorldPos, offsetMap.Value);

        var otherSidePortal = Spawn(autoPortal.Comp.OtherSideProto, targetMapPos);

        if (_linkedEntity.TryLink(autoPortal, otherSidePortal, true))
            RemComp<CP14ZLevelAutoPortalComponent>(autoPortal);
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
