using Content.Server._CP14.StationDungeonMap.Components;
using Content.Server.Station.Components;
using Content.Server.Station.Systems;
using Content.Shared.Teleportation.Systems;
using Robust.Server.GameObjects;
using Robust.Server.Maps;
using Robust.Shared.Map;
using Robust.Shared.Utility;

namespace Content.Server._CP14.StationDungeonMap.EntitySystems;

public sealed partial class CP14StationZLevelsSystem : EntitySystem
{
    [Dependency] private readonly MapSystem _map = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly MapLoaderSystem _mapLoader = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly LinkedEntitySystem _linkedEntity = default!;

    public override void Initialize()
    {
        base.Initialize();
        AutoPortalInitialize();
    }

    private bool TryAddMapInZLevelGroup(Entity<CP14ZLevelGroupComponent> zLevelGroup, int zLevel,  MapId mapId)
    {
        if (!_map.MapExists(mapId))
            return false;

        var mapUid = _map.GetMap(mapId);
        if (!zLevelGroup.Comp.Levels.TryAdd(zLevel, mapId))
            return false;

        var lvlElement = EnsureComp<CP14ZLevelElementComponent>(mapUid);

        lvlElement.Group = zLevelGroup;
        return true;
    }

    private void RemoveMapFromZLevelGroup(Entity<CP14ZLevelGroupComponent> zLevelGroup, int zLevel)
    {
        if (!zLevelGroup.Comp.Levels.ContainsKey(zLevel))
            return;

        var map = zLevelGroup.Comp.Levels[zLevel];

        var mapUid = _map.GetMap(map);

        RemCompDeferred<CP14ZLevelElementComponent>(mapUid);
        zLevelGroup.Comp.Levels.Remove(zLevel);
    }

    private MapId? GetZLevelMap(Entity<CP14ZLevelGroupComponent> zLevelGroup, int zLevel)
    {
        if (!zLevelGroup.Comp.Levels.TryGetValue(zLevel, out var mapId))
            return null;

        return mapId;
    }

    private Entity<CP14ZLevelGroupComponent> CreateZLevelGroup(Dictionary<int, MapId>? maps = null)
    {
        var groupEntity = Spawn(null, MapCoordinates.Nullspace);
        var groupComp = AddComp<CP14ZLevelGroupComponent>(groupEntity);

        if (maps is not null)
        {
            foreach (var (level, map) in maps)
            {
                TryAddMapInZLevelGroup((groupEntity, groupComp), level, map);
            }
        }

        return (groupEntity, groupComp);
    }

    //public MapId? GetMapOffset(EntityUid mapUid, int offset)
    //{
    //    var query = EntityQueryEnumerator<CP14StationZLevelsComponent, StationDataComponent>();
    //    while (query.MoveNext(out var uid, out var zLevel, out _))
    //    {
    //        if (!zLevel.LevelEntities.TryGetValue(Transform(mapUid).MapID, out var currentLevel))
    //            continue;
//
    //        var targetLevel = currentLevel + offset;
//
    //        if (!zLevel.LevelEntities.ContainsValue(targetLevel))
    //            continue;
//
    //        foreach (var (key, value) in zLevel.LevelEntities)
    //        {
    //            if (value == targetLevel && _map.MapExists(key))
    //                return key;
    //        }
    //    }
    //    return null;
    //}
}
