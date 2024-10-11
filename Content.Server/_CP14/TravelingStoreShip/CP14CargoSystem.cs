using System.Numerics;
using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Events;
using Content.Server.Shuttles.Systems;
using Content.Server.Station.Events;
using Content.Server.Station.Systems;
using Content.Shared._CP14.TravelingStoreShip;
using Content.Shared._CP14.TravelingStoreShip.Prototype;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._CP14.TravelingStoreShip;

public sealed partial class CP14CargoSystem : CP14SharedCargoSystem
{
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly MapLoaderSystem _loader = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly ShuttleSystem _shuttles = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly UserInterfaceSystem _userInterface = default!;
    [Dependency] private readonly StationSystem _station = default!;

    public override void Initialize()
    {
        base.Initialize();
        InitializeStore();

        SubscribeLocalEvent<CP14StationTravelingStoreshipTargetComponent, StationPostInitEvent>(OnPostInit);
        SubscribeLocalEvent<CP14TravelingStoreShipComponent, FTLCompletedEvent>(OnFTLCompleted);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<CP14StationTravelingStoreshipTargetComponent>();
        while (query.MoveNext(out var uid, out var ship))
        {
            if (_timing.CurTime < ship.NextTravelTime || ship.NextTravelTime == TimeSpan.Zero)
                continue;

            ship.NextTravelTime = _timing.CurTime + ship.TravelPeriod;

            if (Transform(ship.Shuttle).MapUid == Transform(ship.TradepostMap).MapUid) //Landed on tradepost
            {
                ship.OnStation = false;
                TravelToStation((uid, ship), 15);
            }
            else //Landed on station
            {
                ship.OnStation = true;
                TravelToTradepost((uid, ship), 15);
            }
        }
    }

    private void OnPostInit(Entity<CP14StationTravelingStoreshipTargetComponent> station, ref StationPostInitEvent args)
    {
        if (!Deleted(station.Comp.Shuttle))
            return;

        var tradepostMap = _mapManager.CreateMap();

        if (!_loader.TryLoad(tradepostMap, station.Comp.ShuttlePath.ToString(), out var shuttleUids))
            return;

        var shuttle =  shuttleUids[0];
        station.Comp.Shuttle = shuttle;
        station.Comp.TradepostMap = _mapManager.GetMapEntityId(tradepostMap);
        var travelingStoreShipComp = EnsureComp<CP14TravelingStoreShipComponent>(station.Comp.Shuttle);
        travelingStoreShipComp.Station = station;

        TravelToStation(station, 5); //Start fast travel
        UpdateStorePositions(station);
    }

    private void OnFTLCompleted(Entity<CP14TravelingStoreShipComponent> ent, ref FTLCompletedEvent args)
    {
        if (!TryComp<CP14StationTravelingStoreshipTargetComponent>(ent.Comp.Station, out var station))
            return;

        station.NextTravelTime = _timing.CurTime + station.TravelPeriod;

        if (Transform(ent).MapUid == Transform(station.TradepostMap).MapUid)
        {
            foreach (var position in _proto.EnumeratePrototypes<CP14StoreBuyPositionPrototype>())
            {
                foreach (var pos in position.Services)
                {
                    pos.Buy(EntityManager, ent.Comp.Station);
                }
            }

            SellingThings((ent.Comp.Station, station));
            UpdateStorePositions((ent.Comp.Station, station));
        }
    }

    private void TravelToStation(Entity<CP14StationTravelingStoreshipTargetComponent> station, float flyTime)
    {
        var targetPoints = new List<EntityUid>();
        var targetEnumerator = EntityQueryEnumerator<CP14TravelingStoreShipFTLTargetComponent, TransformComponent>(); //TODO - different method position location
        while (targetEnumerator.MoveNext(out var uid, out _, out _))
        {
            targetPoints.Add(uid);
        }
        if (targetPoints.Count == 0)
            return;

        var target = _random.Pick(targetPoints);

        if (!HasComp<TransformComponent>(station.Comp.Shuttle))
            return;

        var shuttleComp = Comp<ShuttleComponent>(station.Comp.Shuttle);

        var targetPos = _transform.GetWorldPosition(target);
        var mapUid = _transform.GetMap(target);
        if (mapUid == null)
            return;

        _shuttles.FTLToCoordinates(station.Comp.Shuttle, shuttleComp, new EntityCoordinates(mapUid.Value, targetPos), Transform(target).LocalRotation, hyperspaceTime: flyTime, startupTime: 5f);
    }

    private void TravelToTradepost(Entity<CP14StationTravelingStoreshipTargetComponent> station, float flyTime)
    {
        var shuttleComp = Comp<ShuttleComponent>(station.Comp.Shuttle);

        _shuttles.FTLToCoordinates(station.Comp.Shuttle, shuttleComp, new EntityCoordinates(station.Comp.TradepostMap, Vector2.Zero), Angle.Zero, hyperspaceTime: flyTime, startupTime: 5f);
    }

    private void UpdateStorePositions(Entity<CP14StationTravelingStoreshipTargetComponent> station)
    {
        station.Comp.CurrentBuyPositions.Clear();
        station.Comp.CurrentSellPositions.Clear();

        //Static add
        foreach (var position in station.Comp.StaticBuyPositions)
        {
            if (!_proto.TryIndex(position, out var indexedP))
                continue;
            station.Comp.CurrentBuyPositions.Add(position, indexedP.Price.Next(_random));
        }
        foreach (var position in station.Comp.StaticSellPositions)
        {
            if (!_proto.TryIndex(position, out var indexedP))
                continue;
            station.Comp.CurrentSellPositions.Add(position, indexedP.Price.Next(_random));
        }
        //Dynamic add

    }

    private void SellingThings(Entity<CP14StationTravelingStoreshipTargetComponent> station)
    {
        var shuttle = station.Comp.Shuttle;

        var query = EntityQueryEnumerator<CP14SellingPalettComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var selling, out var xform))
        {

        }
    }
}
