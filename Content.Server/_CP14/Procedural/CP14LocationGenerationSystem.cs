using System.Threading;
using Content.Server._CP14.Procedural.Demiplane;
using Content.Server._CP14.Procedural.GlobalWorld.Components;
using Content.Server.Procedural;
using Content.Server.Station.Components;
using Content.Server.Station.Events;
using Content.Server.Station.Systems;
using Content.Shared._CP14.Procedural.Prototypes;
using Robust.Client.GameObjects;
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
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    private ISawmill _sawmill = null!;

    private const double JobMaxTime = 0.002;
    private readonly JobQueue _expeditionQueue = new();
    private readonly List<(CP14SpawnProceduralLocationJob Job, CancellationTokenSource CancelToken)> _jobs = new();

    public override void Initialize()
    {
        base.Initialize();

        _sawmill = _logManager.GetSawmill("cp14_procedural");

        SubscribeLocalEvent<CP14ActiveJobGenerationComponent, ComponentShutdown>(OnGenerationShutdown);
        SubscribeLocalEvent<CP14StationProceduralLocationComponent, StationPostInitEvent>(OnStationPostInit);
    }

    private void OnStationPostInit(Entity<CP14StationProceduralLocationComponent> ent, ref StationPostInitEvent args)
    {
        if (!TryComp<StationDataComponent>(ent, out var stationData))
        {
            _sawmill.Error($"Station {ent} does not have a StationDataComponent, cannot generate location on it.");
            return;
        }

        var largestStationGrid = _station.GetLargestGrid(stationData);

        if (largestStationGrid is null)
        {
            _sawmill.Error($"No grid found for station {ent} to generate location on.");
            return;
        }

        var mapId = _transform.GetMapId(largestStationGrid.Value);

        GenerateLocation(largestStationGrid.Value, mapId, ent.Comp.Location, ent.Comp.Modifiers);
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
                        var ev = new CP14LocationGeneratedEvent();
                        RaiseLocalEvent(job.MapUid, ev);
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
