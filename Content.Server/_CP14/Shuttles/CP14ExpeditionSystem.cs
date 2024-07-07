using Content.Server._CP14.Shuttles.Components;
using Content.Server.DeviceNetwork.Systems;
using Content.Server.GameTicking;
using Content.Server.Parallax;
using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Systems;
using Content.Server.Station.Events;
using Content.Server.Station.Systems;
using Robust.Server.GameObjects;
using Robust.Shared.Console;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._CP14.Shuttles;

public sealed class CP14ExpeditionSystem : EntitySystem
{
    [Dependency] private readonly IConsoleHost _console = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly IPrototypeManager _protoManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly BiomeSystem _biomes = default!;
    [Dependency] private readonly GameTicker _ticker = default!;
    [Dependency] private readonly MapLoaderSystem _loader = default!;
    [Dependency] private readonly DeviceNetworkSystem _deviceNetworkSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly ShuttleSystem _shuttles = default!;
    [Dependency] private readonly StationSpawningSystem _stationSpawning = default!;
    [Dependency] private readonly StationSystem _station = default!;

    public override void Initialize()
    {
        base.Initialize();


        SubscribeLocalEvent<CP14StationExpeditionTargetComponent, StationPostInitEvent>(OnStationPostInit);
    }

    private void OnStationPostInit(Entity<CP14StationExpeditionTargetComponent> station, ref StationPostInitEvent args)
    {
        // If it's a latespawn station then this will fail but that's okey
        SetupExpeditionShip(station);
    }

    private void SetupExpeditionShip(Entity<CP14StationExpeditionTargetComponent> station)
    {
        if (!Deleted(station.Comp.Shuttle))
            return;

        var dummyMap = _mapManager.CreateMap();

        if (_loader.TryLoad(dummyMap, station.Comp.ShuttlePath.ToString(), out var shuttleUids))
        {
            var shuttle =  shuttleUids[0];
            station.Comp.Shuttle = shuttle;
            var shuttleComp = Comp<ShuttleComponent>(station.Comp.Shuttle);
            var expeditionShipComp = EnsureComp<CP14ExpeditionShipComponent>(station.Comp.Shuttle);
            expeditionShipComp.Station = station;

            var targetPoints = new List<EntityUid>();
            var targetEnumerator = EntityQueryEnumerator<CP14ExpeditionShipFTLTargetComponent, TransformComponent>();
            while (targetEnumerator.MoveNext(out var uid, out _, out _))
            {
                targetPoints.Add(uid);
            }
            var target = _random.Pick(targetPoints);
            if (!TryComp<TransformComponent>(shuttle, out var xform))
                return;

            var targetPos = _transform.GetWorldPosition(target);
            var mapUid = _transform.GetMap(target);
            if (mapUid == null)
                return;

            _transform.SetCoordinates(shuttle, xform, new EntityCoordinates(mapUid.Value, targetPos));
            //_shuttles.TryFTLProximity(shuttle, target);
        }
    }
}
