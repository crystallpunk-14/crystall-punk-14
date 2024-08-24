using Content.Server.Station.Components;
using Content.Server.Station.Events;
using Content.Server.Station.Systems;
using Robust.Server.GameObjects;
using Robust.Server.Maps;
using Robust.Shared.Map;

namespace Content.Server._CP14.StationDungeonMap;

public sealed partial class CP14StationZLevelsSystem : EntitySystem
{
    [Dependency] private readonly MapSystem _map = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly MapLoaderSystem _mapLoader = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14StationZLevelsComponent, StationPostInitEvent>(OnPostInit);
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
