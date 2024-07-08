using Content.Server._CP14.Shuttles.Components;
using Content.Server.DeviceNetwork.Systems;
using Content.Server.GameTicking;
using Content.Server.Parallax;
using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Events;
using Content.Server.Shuttles.Systems;
using Content.Server.Spawners.Components;
using Content.Server.Station.Events;
using Content.Server.Station.Systems;
using Content.Shared.CCVar;
using Content.Shared.Movement.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Configuration;
using Robust.Shared.Console;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._CP14.Shuttles;

public sealed class CP14ExpeditionSystem : EntitySystem
{

    [Dependency] private readonly IConfigurationManager _cfgManager = default!;
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


    /// <summary>
    /// Flags if all players must arrive via the Arrivals system, or if they can spawn in other ways.
    /// </summary>
    public float ArrivalTime { get; private set; }

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14StationExpeditionTargetComponent, StationPostInitEvent>(OnStationPostInit);

        SubscribeLocalEvent<CP14StationExpeditionTargetComponent, FTLCompletedEvent>(OnArrivalsDocked);

        ArrivalTime = _cfgManager.GetCVar(CCVars.CP14ExpeditionArrivalTime);
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

            _shuttles.FTLToCoordinates(shuttle, shuttleComp, new EntityCoordinates(mapUid.Value, targetPos), Angle.Zero, hyperspaceTime: ArrivalTime);
        }
    }

    private void OnArrivalsDocked(Entity<CP14StationExpeditionTargetComponent> ent, ref FTLCompletedEvent args)
    {
        //Some announsement logic?
    }

    private bool TryGetExpeditionShip(out EntityUid uid)
    {
        var arrivalsQuery = EntityQueryEnumerator<CP14ExpeditionShipComponent>();

        while (arrivalsQuery.MoveNext(out uid, out _))
        {
            return true;
        }

        return false;
    }

    public void HandlePlayerSpawning(PlayerSpawningEvent ev)
    {
        if (ev.SpawnResult != null)
            return;

        if (!HasComp<CP14ExpeditionShipComponent>(ev.Station))
            return;

        TryGetExpeditionShip(out var ship);

        if (!TryComp(ship, out TransformComponent? shipXform))
            return;

        var gridUid = shipXform.GridUid;

        var points = EntityQueryEnumerator<SpawnPointComponent, TransformComponent>();
        var possiblePositions = new List<EntityCoordinates>();
        while (points.MoveNext(out var uid, out var spawnPoint, out var xform))
        {

            if (spawnPoint.SpawnType != SpawnPointType.LateJoin || xform.GridUid != gridUid)
                continue;

            possiblePositions.Add(xform.Coordinates);
        }

        if (possiblePositions.Count <= 0)
            return;

        var spawnLoc = _random.Pick(possiblePositions);
        ev.SpawnResult = _stationSpawning.SpawnPlayerMob(
            spawnLoc,
            ev.Job,
            ev.HumanoidCharacterProfile,
            ev.Station);

        EnsureComp<PendingClockInComponent>(ev.SpawnResult.Value);
        EnsureComp<AutoOrientComponent>(ev.SpawnResult.Value);
    }
}
