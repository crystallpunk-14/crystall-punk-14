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
    private EntityQuery<ArrivalsBlacklistComponent> _blacklistQuery;
    private void InitializeShuttle()
    {
        _blacklistQuery = GetEntityQuery<ArrivalsBlacklistComponent>();
        SubscribeLocalEvent<CP14TravelingStoreShipComponent, FTLCompletedEvent>(OnFTLCompleted);
    }

    private void UpdateShuttle()
    {
        var query = EntityQueryEnumerator<CP14StationTravelingStoreShipTargetComponent>();
        while (query.MoveNext(out var uid, out var ship))
        {
            if (_timing.CurTime < ship.NextTravelTime || ship.NextTravelTime == TimeSpan.Zero)
                continue;

            if (Transform(ship.Shuttle).MapUid == Transform(ship.TradePostMap).MapUid)
            { //Landed on tradepost
                SendShuttleToStation((uid, ship));
            }
            else
            { //Landed on station
                SendShuttleToTradepost((uid, ship));
            }
        }
    }

    private void SendShuttleToStation(Entity<CP14StationTravelingStoreShipTargetComponent> station)
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
        var targetXform = Transform(target);

        var shuttleComp = Comp<ShuttleComponent>(station.Comp.Shuttle);

        _shuttles.FTLToCoordinates(station.Comp.Shuttle, shuttleComp, targetXform.Coordinates, targetXform.LocalRotation, hyperspaceTime: 5f, startupTime: 0f);
    }

    private void SendShuttleToTradepost(Entity<CP14StationTravelingStoreShipTargetComponent> station)
    {
        var shuttleComp = Comp<ShuttleComponent>(station.Comp.Shuttle);

        _shuttles.FTLToCoordinates(station.Comp.Shuttle, shuttleComp, new EntityCoordinates(station.Comp.TradePostMap, Vector2.Zero), Angle.Zero, hyperspaceTime: 5f);
    }

    private void OnFTLCompleted(Entity<CP14TravelingStoreShipComponent> ent, ref FTLCompletedEvent args)
    {
        if (!TryComp<CP14StationTravelingStoreShipTargetComponent>(ent.Comp.Station, out var station))
            return;

        if (Transform(ent).MapUid == Transform(station.TradePostMap).MapUid)  //Landed on tradepost
        {
            station.NextTravelTime = _timing.CurTime + station.TradePostWaitTime;
            station.OnStation = false;

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
        else   //Landed on station
        {
            station.NextTravelTime = _timing.CurTime + station.StationWaitTime;
            station.OnStation = true;
        }
        UpdateAllStores();
    }
}
