
using Content.Server.Station.Events;
using Content.Server.Station.Systems;
using Content.Shared.Roles;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._CP14.RandomJobs;

public sealed partial class CP14StationRandomJobsSystem : EntitySystem
{
    [Dependency] private readonly StationJobsSystem _jobs = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StationInitializedEvent>(OnInit, after: new[] { typeof(StationJobsSystem) });
    }

    private void OnInit(StationInitializedEvent args)
    {
        if (!TryComp<CP14StationRandomJobsComponent>(args.Station, out var randomJobs))
            return;

        foreach (var entry in randomJobs.Entries)
        {
            if (!_random.Prob(entry.Prob))
                continue;

            var count = entry.Count.Next(_random);

            var tempList = new List<ProtoId<JobPrototype>>(entry.Jobs);

            for (var i = 0; i < count; i++)
            {
                if (tempList.Count == 0)
                    break;

                var job = _random.Pick(tempList);
                tempList.Remove(job);

                if (!_proto.TryIndex(job, out var jobProto))
                    continue;

                _jobs.TryAdjustJobSlot(args.Station, jobProto, 1, createSlot: true);
            }
        }
    }
}
