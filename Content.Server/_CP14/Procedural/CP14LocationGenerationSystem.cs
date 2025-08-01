using System.Threading;
using Content.Server.Procedural;
using Content.Shared._CP14.Demiplane.Prototypes;
using Robust.Shared.CPUJob.JobQueues;
using Robust.Shared.CPUJob.JobQueues.Queues;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._CP14.Procedural;

public sealed class CP14LocationGenerationSystem : EntitySystem
{
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly ILogManager _logManager = default!;
    [Dependency] private readonly DungeonSystem _dungeon = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    private const double JobMaxTime = 0.002;
    private readonly JobQueue _expeditionQueue = new();
    private readonly List<(CP14SpawnProceduralLocationJob Job, CancellationTokenSource CancelToken)> _expeditionJobs = new();

    public override void Update(float frameTime)
    {
        _expeditionQueue.Process();

        foreach (var (job, cancelToken) in _expeditionJobs.ToArray())
        {
            switch (job.Status)
            {
                case JobStatus.Finished:
                    _expeditionJobs.Remove((job, cancelToken));
                    break;
            }
        }
    }

    public void GenerateLocation(EntityUid mapUid, MapId mapId, ProtoId<CP14DemiplaneLocationPrototype> location, List<ProtoId<CP14DemiplaneModifierPrototype>> modifiers, Vector2i position = new(), int? seed = null)
    {
        var cancelToken = new CancellationTokenSource();

        var job = new CP14SpawnProceduralLocationJob(
            JobMaxTime,
            EntityManager,
            _logManager,
            _proto,
            _dungeon,
            _mapSystem,
            mapUid,
            mapId,
            position,
            location,
            modifiers,
            seed ?? _random.Next(-10000, 10000),
            cancelToken.Token);

        _expeditionJobs.Add((job, cancelToken));
        _expeditionQueue.EnqueueJob(job);
    }


    //Need implement this:

    //We stop asynchronous generation of a demiplane early if for some reason this demiplane is deleted before generation is complete
    //foreach (var (job, cancelToken) in _expeditionJobs.ToArray())
    //{
    //    if (job.DemiplaneMapUid == demiplane.Owner)
    //    {
    //        cancelToken.Cancel();
    //        _expeditionJobs.Remove((job, cancelToken));
    //    }
    //}
}
