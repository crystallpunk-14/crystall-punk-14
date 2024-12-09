using System.Globalization;
using System.Linq;
using Content.Server.Chat.Systems;
using Content.Server.Mind;
using Content.Server.Shuttles.Events;
using Content.Server.Station.Systems;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.IdentityManagement.Components;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Roles;
using Content.Shared.Roles.Jobs;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.RoundRemoveShuttle;

public sealed partial class CP14RoundRemoveShuttleSystem : EntitySystem
{
    [Dependency] private readonly ISharedAdminLogManager _adminLog = default!;
    [Dependency] private readonly ChatSystem _chatSystem = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly StationJobsSystem _stationJobs = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14RoundRemoveShuttleComponent, FTLCompletedEvent>(OnFTLComplete);
    }

    private void OnFTLComplete(Entity<CP14RoundRemoveShuttleComponent> ent, ref FTLCompletedEvent args)
    {
        var childrens = Transform(ent).ChildEnumerator;

        HashSet<EntityUid> toDelete = new();
        while (childrens.MoveNext(out var uid))
        {
            if (!_mind.TryGetMind(uid, out _, out var mindComp))
                continue;

            //Trying return all jobs roles
            var userId = mindComp.UserId;
            ProtoId<JobPrototype>? playerJob = null;
            string? jobName = null;
            if (userId is not null)
            {
                RestoreJobs(ent.Comp.Station, userId.Value, out playerJob);
                if (_proto.TryIndex(playerJob, out var indexedJob))
                {
                    jobName = Loc.GetString(indexedJob.Name);
                }
            }

            _adminLog.Add(LogType.Action,
                LogImpact.High,
                $"{ToPrettyString(uid):player} was leave the round on traveling merchant ship");

            _chatSystem.DispatchStationAnnouncement(ent.Comp.Station,
                Loc.GetString(
                     _mobState.IsDead(uid) ? "cp14-earlyleave-ship-announcement-dead" : "cp14-earlyleave-ship-announcement",
                    ("character", mindComp.CharacterName ?? "Unknown"),
                    ("job", CultureInfo.CurrentCulture.TextInfo.ToTitleCase(jobName ?? "Unknown"))
                ),
                Loc.GetString("cp14-ship-sender"),
                playDefaultSound: false
            );
            toDelete.Add(uid);
        }

        while (toDelete.Count > 0)
        {
            var r = toDelete.First();
            toDelete.Remove(r);
            QueueDel(r);
        }
    }

    private void RestoreJobs(EntityUid station, NetUserId userId, out ProtoId<JobPrototype>? outJob)
    {
        outJob = null;
        if (!_stationJobs.TryGetPlayerJobs(station, userId, out var jobs))
            return;

        foreach (var job in jobs)
        {
            _stationJobs.TryAdjustJobSlot(station, job, 1, clamp: true);
            outJob = job;
        }
        _stationJobs.TryRemovePlayerJobs(station, userId);
    }
}
