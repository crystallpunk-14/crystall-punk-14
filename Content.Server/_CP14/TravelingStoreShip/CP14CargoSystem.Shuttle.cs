using System.Numerics;
using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Events;
using Content.Shared._CP14.TravelingStoreShip;
using Content.Shared._CP14.TravelingStoreShip.Prototype;
using Robust.Shared.Map;
using Robust.Shared.Random;

namespace Content.Server._CP14.TravelingStoreShip;

public sealed partial class CP14CargoSystem
{
    private void InitializeShuttle()
    {
        SubscribeLocalEvent<CP14TravelingStoreShipComponent, FTLCompletedEvent>(OnFTLCompleted);
    }

    private void UpdateShuttle()
    {
        var query = EntityQueryEnumerator<CP14StationTravelingStoreshipTargetComponent>();
        while (query.MoveNext(out var uid, out var ship))
        {
            if (_timing.CurTime < ship.NextTravelTime || ship.NextTravelTime == TimeSpan.Zero)
                continue;

            ship.NextTravelTime = _timing.CurTime + ship.TravelPeriod;

            if (Transform(ship.Shuttle).MapUid == Transform(ship.TradepostMap).MapUid) //Landed on tradepost
            {
                ship.OnStation = false;
                SendShuttleToStation((uid, ship), 15);
            }
            else //Landed on station
            {
                ship.OnStation = true;
                SendShuttleToTradepost((uid, ship), 15);
            }
        }
    }

    private void SendShuttleToStation(Entity<CP14StationTravelingStoreshipTargetComponent> station, float flyTime)
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

    private void SendShuttleToTradepost(Entity<CP14StationTravelingStoreshipTargetComponent> station, float flyTime)
    {
        var shuttleComp = Comp<ShuttleComponent>(station.Comp.Shuttle);

        _shuttles.FTLToCoordinates(station.Comp.Shuttle, shuttleComp, new EntityCoordinates(station.Comp.TradepostMap, Vector2.Zero), Angle.Zero, hyperspaceTime: flyTime, startupTime: 5f);
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
}
