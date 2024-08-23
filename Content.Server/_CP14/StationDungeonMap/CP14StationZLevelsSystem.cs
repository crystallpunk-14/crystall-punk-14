using Content.Server.Parallax;
using Content.Server.Station.Components;
using Content.Server.Station.Events;
using Content.Server.Station.Systems;
using Content.Shared.Teleportation.Systems;
using Robust.Server.GameObjects;
using Robust.Server.Maps;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.StationDungeonMap;

public sealed partial class CP14StationZLevelsSystem : EntitySystem
{
    [Dependency] private readonly BiomeSystem _biome = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly MapSystem _map = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly LinkedEntitySystem _linkedEntity = default!;
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
            return;


        foreach (var level in ent.Comp.Levels)
        {
            var path = level.Value.Path.ToString();
            if (path is null)
            {
                Log.Error($"Failed to load map for CP14StationZLevelsSystem at level {level.Key}!");
                continue;
            }

            var mapUid = _map.CreateMap(out var mapId);
            Log.Info($"Created map {mapId} for CP14StationZLevelsSystem at level {level.Key}");
            var options = new MapLoadOptions { LoadMap = true };


            if (!_mapLoader.TryLoad(mapId, path, out var roots, options))
            {
                Log.Error($"Failed to load map for CP14StationZLevelsSystem at level {level.Key}!");
                Del(mapUid);
                return;
            }

            level.Value.GeneratedMap = mapId;
        }
    }
}
