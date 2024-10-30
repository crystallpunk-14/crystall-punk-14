using System.Threading;
using Content.Server._CP14.Demiplan.Components;
using Content.Server._CP14.Demiplan.Jobs;
using Content.Server.Parallax;
using Content.Server.Procedural;
using Content.Shared._CP14.Expeditions.Prototypes;
using Content.Shared.Interaction.Events;
using Robust.Shared.CPUJob.JobQueues;
using Robust.Shared.CPUJob.JobQueues.Queues;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._CP14.Demiplan;

public sealed partial class CP14DemiplanSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ILogManager _logManager = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly BiomeSystem _biome = default!;
    [Dependency] private readonly DungeonSystem _dungeon = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private readonly JobQueue _expeditionQueue = new();
    private readonly List<(CP14SpawnRandomDemiplanJob Job, CancellationTokenSource CancelToken)> _expeditionJobs = new();
    private const double JobMaxTime = 0.002;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14CreateDemiplanOnInteractComponent, MapInitEvent>(GeneratorMapInit);
        SubscribeLocalEvent<CP14CreateDemiplanOnInteractComponent, UseInHandEvent>(GeneratorUsedInHand);

        SubscribeLocalEvent<CP14DemiplanComponent, MapInitEvent>(OnDemiplanInit);
        SubscribeLocalEvent<CP14DemiplanComponent, ComponentShutdown>(OnDemiplanShutdown);
    }

    private void OnDemiplanShutdown(Entity<CP14DemiplanComponent> ent, ref ComponentShutdown args)
    {
        foreach (var (job, cancelToken) in _expeditionJobs.ToArray())
        {
            if (job.DemiplanMapUid == ent.Owner)
            {
                cancelToken.Cancel();
                _expeditionJobs.Remove((job, cancelToken));
            }
        }
    }

    private void OnDemiplanInit(Entity<CP14DemiplanComponent> ent, ref MapInitEvent args)
    {
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var currentTime = _timing.CurTime;
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

    private void GeneratorUsedInHand(Entity<CP14CreateDemiplanOnInteractComponent> generator, ref UseInHandEvent args)
    {
        SpawnRandomDemiplan(generator, out var mapUid);
    }

    private void GeneratorMapInit(Entity<CP14CreateDemiplanOnInteractComponent> generator, ref MapInitEvent args)
    {
        //We select random acceptable expedition config on generator init

        HashSet<CP14DemiplanLocationPrototype> suitableConfigs = new();
        foreach (var expeditionConfig in _proto.EnumeratePrototypes<CP14DemiplanLocationPrototype>())
        {
            suitableConfigs.Add(expeditionConfig);
        }

        if (suitableConfigs.Count == 0)
        {
            Log.Error("Expedition mission generation failed: No suitable location configs.");
            QueueDel(generator);
            return;
        }

        var selectedConfig = _random.Pick(suitableConfigs);
        generator.Comp.LocationConfig = selectedConfig;
    }

    private void SpawnRandomDemiplan(Entity<CP14CreateDemiplanOnInteractComponent> generator, out EntityUid mapUid)
    {
        mapUid = _mapSystem.CreateMap(out var mapId, runMapInit: false);

        var cancelToken = new CancellationTokenSource();
        var job = new CP14SpawnRandomDemiplanJob(
            JobMaxTime,
            EntityManager,
            _logManager,
            _mapManager,
            _proto,
            _dungeon,
            _metaData,
            _mapSystem,
            mapUid,
            mapId,
            generator.Comp.LocationConfig,
            _random.Next(-10000, 10000),
            cancelToken.Token);

        _expeditionJobs.Add((job, cancelToken));
        _expeditionQueue.EnqueueJob(job);
    }
}
