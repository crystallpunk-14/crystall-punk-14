using System.Globalization;
using System.Linq;
using System.Numerics;
using Content.Server.Chat.Systems;
using Content.Server.Mind;
using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Events;
using Content.Server.Station.Systems;
using Content.Shared._CP14.Cargo;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.Roles.Jobs;
using Robust.Shared.Map;
using Robust.Shared.Random;

namespace Content.Server._CP14.Cargo;

public sealed partial class CP14CargoSystem
{
    [Dependency] private readonly ISharedAdminLogManager _adminLog = default!;
    [Dependency] private readonly ChatSystem _chatSystem = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly SharedJobSystem _job = default!;
    [Dependency] private readonly StationJobsSystem _stationJobs = default!;

    private void InitializeShuttle()
    {
        SubscribeLocalEvent<CP14TravelingStoreShipComponent, FTLCompletedEvent>(OnFTLCompleted);
    }

    private void UpdateShuttle()
    {
        var query = EntityQueryEnumerator<CP14StationTravelingStoreShipTargetComponent>();
        while (query.MoveNext(out var uid, out var ship))
        {
            if (_timing.CurTime < ship.NextTravelTime || ship.NextTravelTime == TimeSpan.Zero)
                continue;

            if (ship.Shuttle is null || ship.TradePostMap is null)
                continue;

            if (Transform(ship.Shuttle.Value).MapUid == Transform(ship.TradePostMap.Value).MapUid)
            {
                // if landed on trade post
                ship.NextTravelTime = _timing.CurTime + ship.StationWaitTime;
                SendShuttleToStation(ship.Shuttle.Value);
            }
            else
            {
                // if landed on station
                ship.NextTravelTime = _timing.CurTime + ship.TradePostWaitTime;
                SendShuttleToTradepost(ship.Shuttle.Value, ship.TradePostMap.Value);
            }
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

        _shuttles.FTLToCoordinates(shuttle,
            shuttleComp,
            targetXform.Coordinates,
            targetXform.LocalRotation,
            hyperspaceTime: 20f,
            startupTime: startupTime);
    }

    private void SendShuttleToTradepost(EntityUid shuttle, EntityUid tradePostMap)
    {
        var shuttleComp = Comp<ShuttleComponent>(shuttle);

        _shuttles.FTLToCoordinates(shuttle,
            shuttleComp,
            new EntityCoordinates(tradePostMap, Vector2.Zero),
            Angle.Zero,
            hyperspaceTime: 20f);
    }

    private void OnFTLCompleted(Entity<CP14TravelingStoreShipComponent> ent, ref FTLCompletedEvent args)
    {
        if (!TryComp<CP14StationTravelingStoreShipTargetComponent>(ent.Comp.Station, out var station))
            return;

        if (station.TradePostMap is not null &&
            Transform(ent).MapUid == Transform(station.TradePostMap.Value).MapUid) //Landed on tradepost
        {
            station.OnStation = false;

            var b = station.Balance;

            PlayersPurgeJobs(ent);

            SellingThings((ent.Comp.Station, station)); // +balance
            TopUpBalance((ent.Comp.Station, station)); //+balance
            BuyToQueue((ent.Comp.Station, station)); //-balance +buyQueue
            TrySpawnBuyedThings((ent.Comp.Station, station));
            UpdateStorePositions((ent.Comp.Station, station));
        }
        else //Landed on station
        {
            station.OnStation = true;

            CashOut((ent.Comp.Station, station));
        }

        UpdateAllStores();
    }

    private void PlayersPurgeJobs(Entity<CP14TravelingStoreShipComponent> ent)
    {
        var childrens = Transform(ent).ChildEnumerator;

        HashSet<EntityUid> toDelete = new();
        while (childrens.MoveNext(out var uid))
        {
            if (!_mind.TryGetMind(uid, out var mindId, out var mindComp))
                continue;

            if (!_job.MindTryGetJob(mindId, out var jobProto))
                continue;

            _adminLog.Add(LogType.Action,
                LogImpact.High,
                $"{ToPrettyString(uid):player} was leave the round on traveling merchant ship");

            _chatSystem.DispatchStationAnnouncement(ent.Comp.Station,
                Loc.GetString(
                    "cp14-earlyleave-ship-announcement",
                    ("character", mindComp.CharacterName ?? "Unknown"),
                    ("entity", ent.Owner), // gender things for supporting downstreams with other languages
                    ("job", CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Loc.GetString(jobProto.Name)))
                ),
                Loc.GetString("cp14-ship-sender"),
                playDefaultSound: false
            );

            _stationJobs.TryAdjustJobSlot(ent.Comp.Station, jobProto, 1, clamp: true);
            toDelete.Add(uid);
        }

        while (toDelete.Count > 0)
        {
            var r = toDelete.First();
            toDelete.Remove(r);
            QueueDel(r);
        }
    }
}
