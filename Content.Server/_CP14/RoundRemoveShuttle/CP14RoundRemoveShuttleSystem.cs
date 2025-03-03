using System.Globalization;
using System.Linq;
using Content.Server.Chat.Systems;
using Content.Server.Mind;
using Content.Server.Shuttles.Events;
using Content.Server.Station.Systems;
using Content.Server.StationRecords;
using Content.Server.StationRecords.Systems;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.StationRecords;
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
    [Dependency] private readonly StationRecordsSystem _stationRecords = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;

    private readonly HashSet<Entity<MindContainerComponent>> _mindSet = new();
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14RoundRemoveShuttleComponent, FTLStartedEvent>(OnFTL);
    }

    private void OnFTL(Entity<CP14RoundRemoveShuttleComponent> ent, ref FTLStartedEvent args)
    {
        _mindSet.Clear();
        _lookup.GetChildEntities(ent, _mindSet);

        if (!TryComp<StationRecordsComponent>(ent.Comp.Station, out var stationRecords))
            return;

        HashSet<EntityUid> toDelete = new();
        var query = EntityQueryEnumerator<MindContainerComponent>();
        while (query.MoveNext(out var uid, out var mindContainer))
        {
            if (Transform(uid).GridUid != ent)
                continue;

            if (!_mind.TryGetMind(uid, out _, out var mindComp, container: mindContainer))
                continue;

            var name = Name(uid);
            var recordId = _stationRecords.GetRecordByName(ent.Comp.Station, name);

            if (recordId is null)
                return;

            var key = new StationRecordKey(recordId.Value, ent.Comp.Station);
            if (!_stationRecords.TryGetRecord<GeneralStationRecord>(key, out var entry, stationRecords))
                return;

            _stationRecords.RemoveRecord(key, stationRecords);
            //Trying return all jobs roles
            var userId = mindComp.UserId;
            string? jobName = entry.JobTitle;
            if (userId is not null)
            {
                _stationJobs.TryAdjustJobSlot(ent.Comp.Station, entry.JobPrototype, 1, clamp: true);
                if (_proto.TryIndex(entry.JobPrototype, out var indexedJob))
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
}
