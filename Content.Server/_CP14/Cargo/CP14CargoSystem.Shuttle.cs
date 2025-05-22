using System.Numerics;
using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Events;
using Content.Server.Shuttles.Systems;
using Content.Server.Station.Components;
using Content.Shared._CP14.Cargo;
using Content.Shared._CP14.DayCycle;
using Robust.Shared.Map;
using Robust.Shared.Random;

namespace Content.Server._CP14.Cargo;

public sealed partial class CP14CargoSystem
{
    [Dependency] private readonly ShuttleSystem _shuttle = default!;
    private void InitializeShuttle()
    {
        SubscribeLocalEvent<CP14TravelingStoreShipComponent, FTLCompletedEvent>(OnFTLCompleted);
        SubscribeLocalEvent<CP14StartNightEvent>(OnStartNight);
        SubscribeLocalEvent<CP14StartDayEvent>(OnStartDay);
    }

    private void OnStartNight(CP14StartNightEvent ev)
    {
        if (!HasComp<BecomesStationComponent>(ev.Map))
            return;

        var query = EntityQueryEnumerator<CP14StationTravelingStoreShipComponent>();

        while (query.MoveNext(out var uid, out var ship))
        {
            if (ship.Shuttle is null || ship.TradePostMap is null)
                continue;

            SendShuttleToTradepost(ship.Shuttle.Value, ship.TradePostMap.Value);
        }
    }

    private void OnStartDay(CP14StartDayEvent ev)
    {
        if (!HasComp<BecomesStationComponent>(ev.Map))
            return;
        var query = EntityQueryEnumerator<CP14StationTravelingStoreShipComponent>();

        while (query.MoveNext(out var uid, out var ship))
        {
            if (ship.Shuttle is null || ship.TradePostMap is null)
                continue;

            SendShuttleToStation(ship.Shuttle.Value);
        }
    }

    private void SendShuttleToStation(EntityUid shuttle, float startupTime = 0f)
    {
        var targetPoints = new List<EntityUid>();
        var targetEnumerator =
            EntityQueryEnumerator<CP14TravelingStoreShipFTLTargetComponent,
                TransformComponent>(); //TODO - different method position location
        while (targetEnumerator.MoveNext(out var uid, out _, out _))
        {
            targetPoints.Add(uid);
        }

        if (targetPoints.Count == 0)
            return;

        var target = _random.Pick(targetPoints);
        var targetXform = Transform(target);

        var shuttleComp = Comp<ShuttleComponent>(shuttle);

        _shuttle.FTLToCoordinates(shuttle,
            shuttleComp,
            targetXform.Coordinates,
            targetXform.LocalRotation,
            hyperspaceTime: 20f,
            startupTime: startupTime);
    }

    private void SendShuttleToTradepost(EntityUid shuttle, EntityUid tradePostMap)
    {
        var shuttleComp = Comp<ShuttleComponent>(shuttle);

        _shuttle.FTLToCoordinates(shuttle,
            shuttleComp,
            new EntityCoordinates(tradePostMap, Vector2.Zero),
            Angle.Zero,
            startupTime: 10f,
            hyperspaceTime: 20f);
    }

    private void OnFTLCompleted(Entity<CP14TravelingStoreShipComponent> ent, ref FTLCompletedEvent args)
    {
        if (!TryComp<CP14StationTravelingStoreShipComponent>(ent.Comp.Station, out var station))
            return;

        if (station.TradePostMap is not null &&
            Transform(ent).MapUid == Transform(station.TradePostMap.Value).MapUid) //Landed on tradepost
        {
            station.OnStation = false;

            //SellingThings((ent.Comp.Station, station)); // +balance
            //TopUpBalance((ent.Comp.Station, station)); //+balance
            //BuyToQueue((ent.Comp.Station, station)); //-balance +buyQueue
            //TrySpawnBuyedThings((ent.Comp.Station, station));
            //UpdateStorePositions((ent.Comp.Station, station));
        }
        else //Landed on station
        {
            station.OnStation = true;

            //CashOut((ent.Comp.Station, station));
            station.Balance = 0;
        }
    }
}
