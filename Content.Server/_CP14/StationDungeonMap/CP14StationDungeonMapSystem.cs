using Content.Server.Parallax;
using Content.Server.Station.Components;
using Content.Server.Station.Events;
using Content.Server.Station.Systems;
using Content.Shared.Teleportation.Systems;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.StationDungeonMap;

public sealed partial class CP14StationDungeonMapSystem : EntitySystem
{
    [Dependency] private readonly BiomeSystem _biome = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly MapSystem _map = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly LinkedEntitySystem _linkedEntity = default!;
    [Dependency] private readonly StationSystem _station = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CP14StationDungeonMapComponent, StationPostInitEvent>(OnStationPostInit);
    }

    private void OnStationPostInit(Entity<CP14StationDungeonMapComponent> map, ref StationPostInitEvent args)
    {
        if (!TryComp(map, out StationDataComponent? dataComp))
            return;

        var mapUid = _map.CreateMap(out var mapId);
        _metaData.SetEntityName(mapUid, map.Comp.MapName);

        _biome.EnsurePlanet(mapUid, _proto.Index(map.Comp.Biome), map.Comp.Seed, mapLight: map.Comp.MapLightColor);

        EntityManager.AddComponents(mapUid, map.Comp.Components);

        TryLinkDungeonAndStationMaps(dataComp, mapUid);
    }

    private bool TryLinkDungeonAndStationMaps(StationDataComponent dataComp, EntityUid mapUid)
    {
        var station = _station.GetLargestGrid(dataComp);
        if (station == null)
            return false;

        var stationMapId = Transform(station.Value).MapID;
        _map.TryGetMap(stationMapId, out var stationMapUid);
        if (stationMapUid == null)
            return false;

        return _linkedEntity.TryLink(mapUid, stationMapUid.Value);
    }
}
