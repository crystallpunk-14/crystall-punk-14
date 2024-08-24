using Content.Server._CP14.StationDungeonMap.Components;
using Content.Server.Station.Components;
using Content.Server.Station.Events;
using Content.Server.Station.Systems;
using Content.Shared.Maps;
using Content.Shared.Teleportation.Components;
using Content.Shared.Teleportation.Systems;
using Robust.Server.GameObjects;
using Robust.Server.Maps;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
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

        SubscribeLocalEvent<CP14StationZLevelsComponent, StationPostInitEvent>(OnPostInit);
        SubscribeLocalEvent<CP14ZLevelAutoPortalComponent, MapInitEvent>(OnPortalMapInit);
    }

    private void OnPortalMapInit(Entity<CP14ZLevelAutoPortalComponent> autoPortal, ref MapInitEvent args)
    {
        if (!TryComp<PortalComponent>(autoPortal, out var portalComp))
            return;

        var mapId = Transform(autoPortal).MapID;
        _map.TryGetMap(mapId, out var mapUid);

        var offsetMap = GetMapOffset(mapId, autoPortal.Comp.ZLevelOffset);

        if (offsetMap is null)
            return;

        var currentWorldPos = _transform.GetWorldPosition(autoPortal);
        var targetMapPos = new MapCoordinates(currentWorldPos, offsetMap.Value);

        var otherSidePortal = Spawn(autoPortal.Comp.OtherSideProto, targetMapPos);

        if (_linkedEntity.TryLink(autoPortal, otherSidePortal, true))
            RemComp<CP14ZLevelAutoPortalComponent>(autoPortal);

    }

    private void ClearOtherSide(EntityUid otherSidePortal, EntityUid targetMapUid)
    {
        var tiles = new List<(Vector2i Index, Tile Tile)>();
        var originF = _transform.GetWorldPosition(otherSidePortal);
        var origin = new Vector2i((int) originF.X, (int) originF.Y);
        var tileDef = _tileDefManager["CP14FloorStonebricks"]; //TODO: Remove hardcode
        var seed = _random.Next();
        var random = new Random(seed);
        var grid = Comp<MapGridComponent>(targetMapUid);

        for (var x = -2; x <= 2; x++) //TODO: Remove hardcode
        {
            for (var y = -2; y <= 2; y++)
            {
                tiles.Add((new Vector2i(x, y) + origin, new Tile(tileDef.TileId, variant: _tile.PickVariant((ContentTileDefinition) tileDef, random))));
            }
        }

        _maps.SetTiles(targetMapUid, grid, tiles);
    }

    private void OnPostInit(Entity<CP14StationZLevelsComponent> ent, ref StationPostInitEvent args)
    {
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
            Log.Info($"Created map {mapId} for CP14StationZLevelsSystem at level {map}");
            var options = new MapLoadOptions { LoadMap = true };

            if (!_mapLoader.TryLoad(mapId, path, out _, options))
            {
                Log.Error($"Failed to load map for CP14StationZLevelsSystem at level {map}!");
                Del(mapUid);
                continue;
            }
            ent.Comp.LevelEntities.Add(mapId, map);
        }
    }

    public MapId? GetMapOffset(MapId mapId, int offset)
    {
        var query = EntityQueryEnumerator<CP14StationZLevelsComponent, StationDataComponent>();
        while (query.MoveNext(out _, out var zLevel, out _))
        {
            if (!zLevel.LevelEntities.TryGetValue(mapId, out var currentLevel))
                return null;

            var targetLevel = currentLevel + offset;

            if (!zLevel.LevelEntities.ContainsValue(targetLevel))
                return null;

            foreach (var (key, value) in zLevel.LevelEntities)
            {
                if (value == targetLevel)
                    return key;
            }
        }
        return null;
    }
}
