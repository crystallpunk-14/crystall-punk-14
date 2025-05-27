using System.Globalization;
using Content.Server.Chat.Managers;
using Content.Server.Chat.Systems;
using Content.Server.GameTicking;
using Content.Server.Mind;
using Content.Server.Station.Components;
using Content.Server.Station.Systems;
using Content.Server.StationRecords;
using Content.Server.StationRecords.Systems;
using Content.Shared.Administration.Logs;
using Content.Shared.Chat;
using Content.Shared.Database;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.StationRecords;
using Robust.Shared.Network;
using Robust.Shared.Physics.Events;
using Robust.Shared.Player;

namespace Content.Server._CP14.RoundLeave;

public sealed class CP14RoundLeaveSystem : EntitySystem
{
    [Dependency] private readonly ISharedAdminLogManager _adminLog = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly ChatSystem _chatSystem = default!;
    [Dependency] private readonly StationJobsSystem _stationJobs = default!;
    [Dependency] private readonly StationRecordsSystem _stationRecords = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly MindSystem _mind = default!;

    private EntityQuery<MindContainerComponent> _mindContainerQuery;
    private EntityQuery<CP14RoundLeavingComponent> _leavingQuery;
    private EntityQuery<ActorComponent> _actorQuery;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14RoundLeaveComponent, StartCollideEvent>(OnCollide);
        SubscribeLocalEvent<CP14RoundLeaveComponent, EndCollideEvent>(EndCollide);

        SubscribeLocalEvent<CP14RoundLeavingComponent, MapInitEvent>(OnStartLeaving);
        SubscribeLocalEvent<CP14RoundLeavingComponent, MindRemovedMessage>(OnLeaveRound);

        _mindContainerQuery = GetEntityQuery<MindContainerComponent>();
        _leavingQuery = GetEntityQuery<CP14RoundLeavingComponent>();
        _actorQuery = GetEntityQuery<ActorComponent>();
    }

    private void OnCollide(Entity<CP14RoundLeaveComponent> ent, ref StartCollideEvent args)
    {
        if (!_mindContainerQuery.HasComp(args.OtherEntity))
            return;

        EnsureComp<CP14RoundLeavingComponent>(args.OtherEntity, out var leaving);
        leaving.Leaver.Add(ent);

        //Auto round remove
        if (_mobState.IsDead(args.OtherEntity) && _mind.TryGetMind(args.OtherEntity, out var mindId, out var mindComp) && mindComp.UserId is not null)
        {
            LeaveRound(args.OtherEntity, mindComp.UserId.Value);
        }
    }

    private void EndCollide(Entity<CP14RoundLeaveComponent> ent, ref EndCollideEvent args)
    {
        if (_leavingQuery.TryGetComponent(args.OtherEntity, out var leaving))
        {
            leaving.Leaver.Remove(ent);
            if (leaving.Leaver.Count == 0)
            {
                RemComp<CP14RoundLeavingComponent>(args.OtherEntity);
            }
        }
    }

    private void OnStartLeaving(Entity<CP14RoundLeavingComponent> ent, ref MapInitEvent args)
    {
        var msg = Loc.GetString("cp14-earlyleave-warning");

        if (_actorQuery.TryComp(ent, out var actor))
            _chatManager.ChatMessageToOne(ChatChannel.Server, msg, msg, ent, false, actor.PlayerSession.Channel);
    }

    private void OnLeaveRound(Entity<CP14RoundLeavingComponent> ent, ref MindRemovedMessage args)
    {
        var userId = args.Mind.Comp.UserId;

        if (userId == null)
            return;

        _adminLog.Add(LogType.Action,
            LogImpact.High,
            $"{ToPrettyString(ent):player} left the round by ghosting into mist");

        LeaveRound(ent, userId.Value, args.Mind);
    }

    private void LeaveRound(EntityUid uid, NetUserId userId, Entity<MindComponent>? mind = null)
    {
        var station = _station.GetOwningStation(uid);
        var name = Name(uid);

        if (!TryComp<StationRecordsComponent>(station, out var stationRecords))
            return;

        //Trying return all jobs roles
        foreach (var uniqueStation in _station.GetStationsSet())
        {
            if (!TryComp<StationJobsComponent>(uniqueStation, out var stationJobs))
                continue;

            if (!_stationJobs.TryGetPlayerJobs(uniqueStation, userId, out var jobs, stationJobs))
                continue;

            foreach (var job in jobs)
            {
                _stationJobs.TryAdjustJobSlot(uniqueStation, job, 1, clamp: true);
            }

            _stationJobs.TryRemovePlayerJobs(uniqueStation, userId, stationJobs);
        }

        var jobName = Loc.GetString("earlyleave-cryo-job-unknown");
        var recordId = _stationRecords.GetRecordByName(station.Value, name);
        if (recordId != null)
        {
            var key = new StationRecordKey(recordId.Value, station.Value);
            if (_stationRecords.TryGetRecord<GeneralStationRecord>(key, out var entry, stationRecords))
                jobName = entry.JobTitle;

            _stationRecords.RemoveRecord(key, stationRecords);
        }

        _chatSystem.DispatchStationAnnouncement(station.Value,
            Loc.GetString(
                _mobState.IsAlive(uid) ? "cp14-earlyleave-ship-announcement" : "cp14-earlyleave-ship-announcement-dead",
                ("character", name),
                ("job", CultureInfo.CurrentCulture.TextInfo.ToTitleCase(jobName))
            ),
            Loc.GetString("cp14-ship-sender"),
            playDefaultSound: false
        );

        QueueDel(uid);

        //if (mind is not null && mind.Value.Comp.Session is not null)
        //    _gameTicker.Respawn(mind.Value.Comp.Session);
    }
}
