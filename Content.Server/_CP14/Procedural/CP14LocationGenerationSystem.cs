using System.Threading;
using Content.Server._CP14.Procedural.GlobalWorld.Components;
using Content.Server.Procedural;
using Content.Server.Station.Systems;
using Content.Shared._CP14.Procedural.Prototypes;
using Content.Shared.Procedural;
using JetBrains.Annotations;
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
    private readonly List<(CP14SpawnProceduralLocationJob Job, CancellationTokenSource CancelToken)> _jobs = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14ActiveJobGenerationComponent, ComponentShutdown>(OnGenerationShutdown);

    }

    public override void Update(float frameTime)
    {
        _expeditionQueue.Process();

        foreach (var (job, cancelToken) in _jobs.ToArray())
        {
            switch (job.Status)
            {
                case JobStatus.Finished:
                    if (job.JobName is not null)
                    {
                        var ev = new CP14LocationGeneratedEvent(job.JobName);
                        RaiseLocalEvent(ev);
                    }
                    RemComp<CP14ActiveJobGenerationComponent>(job.MapUid);

                    _jobs.Remove((job, cancelToken));
                    break;
            }
        }
    }

    /// <summary>
    /// Generates a new procedural location on the specified map and coordinates.
    /// Essentially, this is a wrapper for _dungeon.GenerateDungeon, which collects the necessary settings for the
    /// dungeon based on the location and modifiers.
    /// </summary>
    public void GenerateLocation(EntityUid mapUid, MapId mapId, ProtoId<CP14ProceduralLocationPrototype> location, List<ProtoId<CP14ProceduralModifierPrototype>> modifiers, Vector2i position = new(), int? seed = null, string? jobName = null)
    {
        var cancelToken = new CancellationTokenSource();

        EnsureComp<CP14ActiveJobGenerationComponent>(mapUid);

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
            seed ?? _random.Next(-10000, 10000),
            location,
            modifiers,
            jobName,
            cancelToken.Token);

        _jobs.Add((job, cancelToken));
        _expeditionQueue.EnqueueJob(job);
    }


    private void OnGenerationShutdown(Entity<CP14ActiveJobGenerationComponent> ent, ref ComponentShutdown args)
    {
        //We stop asynchronous generation of a location early if for some reason this location is deleted before generation is complete
        foreach (var (job, cancelToken) in _jobs.ToArray())
        {
            if (job.MapUid == ent.Owner)
            {
                cancelToken.Cancel();
                _jobs.Remove((job, cancelToken));
            }
        }
    }
}


[PublicAPI]
public sealed class CP14LocationGeneratedEvent(string jobName) : EntityEventArgs
{
    public string JobName = jobName;
}
