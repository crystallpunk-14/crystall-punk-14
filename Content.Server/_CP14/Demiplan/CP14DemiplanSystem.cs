using System.Threading;
using Content.Server._CP14.Demiplan.Components;
using Content.Server._CP14.Demiplan.Jobs;
using Content.Server.Mind;
using Content.Server.Procedural;
using Content.Shared._CP14.Demiplan;
using Content.Shared._CP14.Demiplan.Components;
using Content.Shared._CP14.Demiplan.Prototypes;
using Content.Shared.Interaction.Events;
using Content.Shared.Throwing;
using Robust.Server.Audio;
using Robust.Shared.CPUJob.JobQueues;
using Robust.Shared.CPUJob.JobQueues.Queues;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._CP14.Demiplan;

public sealed partial class CP14DemiplanSystem : CP14SharedDemiplanSystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ILogManager _logManager = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly DungeonSystem _dungeon = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly MindSystem _mind = default!;

    private readonly JobQueue _expeditionQueue = new();
    private readonly List<(CP14SpawnRandomDemiplanJob Job, CancellationTokenSource CancelToken)> _expeditionJobs = new();
    private const double JobMaxTime = 0.002;

    public override void Initialize()
    {
        base.Initialize();

        InitTeleportation();
        InitConnections();

        SubscribeLocalEvent<CP14DemiplanGeneratorDataComponent, MapInitEvent>(GeneratorMapInit);
        SubscribeLocalEvent<CP14DemiplanGeneratorDataComponent, UseInHandEvent>(GeneratorUsedInHand);

        SubscribeLocalEvent<CP14DemiplanComponent, ComponentShutdown>(OnDemiplanShutdown);
    }

    private void GeneratorMapInit(Entity<CP14DemiplanGeneratorDataComponent> generator, ref MapInitEvent args)
    {
        // Here, a unique Demiplan config should be generated based on the CP14DemiplanGeneratorDataComponent

        //Location generation
        HashSet<CP14DemiplanLocationPrototype> suitableConfigs = new();
        foreach (var locationConfig in _proto.EnumeratePrototypes<CP14DemiplanLocationPrototype>())
        {
            suitableConfigs.Add(locationConfig);
        }

        if (suitableConfigs.Count == 0)
        {
            Log.Error("Expedition mission generation failed: No suitable location configs.");
            QueueDel(generator);
            return;
        }

        var selectedConfig = _random.Pick(suitableConfigs);
        generator.Comp.LocationConfig = selectedConfig;

        //Modifier generation

        //Scenario generation

        //ETC generation
    }

    private void OnDemiplanShutdown(Entity<CP14DemiplanComponent> demiplan, ref ComponentShutdown args)
    {
        foreach (var (job, cancelToken) in _expeditionJobs.ToArray())
        {
            if (job.DemiplanMapUid == demiplan.Owner)
            {
                cancelToken.Cancel();
                _expeditionJobs.Remove((job, cancelToken));
            }
        }

        foreach (var connection in demiplan.Comp.Connections)
        {
            RemoveDemiplanConnection(demiplan, connection);
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        UpdateTeleportation(frameTime);

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

    private void GeneratorUsedInHand(Entity<CP14DemiplanGeneratorDataComponent> generator, ref UseInHandEvent args)
    {
        SpawnRandomDemiplan(generator);

        if (generator.Comp.GeneratedMap is null)
            return;

        //TEST
        var tempRift = EntityManager.Spawn("CP14DemiplanTimedRadiusPassway");
        _transform.SetCoordinates(tempRift, Transform(args.User).Coordinates);

        var connection = EnsureComp<CP14DemiplanConnectionComponent>(tempRift);
        AddDemiplanConnection(generator.Comp.GeneratedMap.Value, (tempRift, connection));
    }

    private void SpawnRandomDemiplan(Entity<CP14DemiplanGeneratorDataComponent> generator)
    {
        var mapUid = _mapSystem.CreateMap(out var mapId, runMapInit: false);
        var demiComp = EntityManager.EnsureComponent<CP14DemiplanComponent>(mapUid);
        generator.Comp.GeneratedMap = (mapUid, demiComp);

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

    public bool TryGetDemiplanEntryPoint(Entity<CP14DemiplanComponent> demiplan, out Entity<CP14DemiplanEntryPointComponent>? entryPoint)
    {
        entryPoint = null;

        if (demiplan.Comp.EntryPoints.Count == 0)
            return false;

        entryPoint = _random.Pick(demiplan.Comp.EntryPoints);
        return true;
    }

    public bool TryGetDemiplanConnection(Entity<CP14DemiplanComponent> demiplan,
        out Entity<CP14DemiplanConnectionComponent>? connection)
    {
        connection = null;

        if (demiplan.Comp.Connections.Count == 0)
            return false;

        connection = _random.Pick(demiplan.Comp.Connections);
        return true;
    }
}
