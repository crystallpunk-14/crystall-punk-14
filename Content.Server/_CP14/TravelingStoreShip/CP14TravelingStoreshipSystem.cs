using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Systems;
using Content.Server.Station.Events;
using Content.Server.Station.Systems;
using Robust.Server.GameObjects;
using Robust.Shared.Configuration;
using Robust.Shared.Map;
using Robust.Shared.Random;

namespace Content.Server._CP14.TravelingStoreShip;

public sealed class CP14TravelingStoreShipSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfgManager = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly MapLoaderSystem _loader = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly ShuttleSystem _shuttles = default!;
    [Dependency] private readonly StationSpawningSystem _stationSpawning = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14StationTravelingStoreshipTargetComponent, StationPostInitEvent>(OnPostInitSetupTravelingStoreShip);
    }

    private void OnPostInitSetupTravelingStoreShip(Entity<CP14StationTravelingStoreshipTargetComponent> station, ref StationPostInitEvent args)
    {
        if (!Deleted(station.Comp.Shuttle))
            return;

        var dummyMap = _mapManager.CreateMap();

        if (!_loader.TryLoad(dummyMap, station.Comp.ShuttlePath.ToString(), out var shuttleUids))
            return;

        var shuttle =  shuttleUids[0];
        station.Comp.Shuttle = shuttle;
        var shuttleComp = Comp<ShuttleComponent>(station.Comp.Shuttle);
        var travelingStoreShipComp = EnsureComp<CP14TravelingStoreShipComponent>(station.Comp.Shuttle);
        travelingStoreShipComp.Station = station;

        var targetPoints = new List<EntityUid>();
        var targetEnumerator = EntityQueryEnumerator<CP14TravelingStoreShipFTLTargetComponent, TransformComponent>(); //TODO - different method position location
        while (targetEnumerator.MoveNext(out var uid, out _, out _))
        {
            targetPoints.Add(uid);
        }
        if (targetPoints.Count == 0)
            return;

        var target = _random.Pick(targetPoints);

        if (!HasComp<TransformComponent>(shuttle))
            return;

        var targetPos = _transform.GetWorldPosition(target);
        var mapUid = _transform.GetMap(target);
        if (mapUid == null)
            return;

        _shuttles.FTLToCoordinates(shuttle, shuttleComp, new EntityCoordinates(mapUid.Value, targetPos), Angle.Zero, hyperspaceTime: 20, startupTime: 0.5f); //roundstart short time
    }
}
