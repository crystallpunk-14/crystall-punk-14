using System.Threading;
using Content.Server._CP14.Expeditions.Components;
using Content.Server._CP14.Expeditions.Jobs;
using Content.Server.Parallax;
using Content.Server.Procedural;
using Content.Shared._CP14.Expeditions;
using Content.Shared._CP14.Expeditions.Prototypes;
using Content.Shared.Interaction.Events;
using Content.Shared.Parallax.Biomes;
using Content.Shared.Procedural;
using Robust.Shared.CPUJob.JobQueues;
using Robust.Shared.CPUJob.JobQueues.Queues;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._CP14.Expeditions;

public sealed partial class CP14ExpeditionSystem : EntitySystem
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
    private readonly List<(CP14SpawnExpeditionJob Job, CancellationTokenSource CancelToken)> _expeditionJobs = new();
    private const double JobMaxTime = 0.002;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14ExpeditionGeneratorComponent, MapInitEvent>(GeneratorMapInit);
        SubscribeLocalEvent<CP14ExpeditionGeneratorComponent, UseInHandEvent>(GeneratorUsedInHand);

        SubscribeLocalEvent<CP14ExpeditionComponent, MapInitEvent>(OnExpeditionInit);
        SubscribeLocalEvent<CP14ExpeditionComponent, ComponentShutdown>(OnExpeditionShutdown);
    }

    private void OnExpeditionShutdown(Entity<CP14ExpeditionComponent> ent, ref ComponentShutdown args)
    {
        foreach (var (job, cancelToken) in _expeditionJobs.ToArray())
        {
            if (job.ExpeditionMap == ent.Owner)
            {
                cancelToken.Cancel();
                _expeditionJobs.Remove((job, cancelToken));
            }
        }
    }

    private void OnExpeditionInit(Entity<CP14ExpeditionComponent> ent, ref MapInitEvent args)
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

    private void GeneratorUsedInHand(Entity<CP14ExpeditionGeneratorComponent> generator, ref UseInHandEvent args)
    {
        SpawnMission(generator);
    }

    private void GeneratorMapInit(Entity<CP14ExpeditionGeneratorComponent> generator, ref MapInitEvent args)
    {
        //We select random acceptable expedition config on generator init

        HashSet<CP14ExpeditionLocationPrototype> suitableConfigs = new();
        foreach (var expeditionConfig in _proto.EnumeratePrototypes<CP14ExpeditionLocationPrototype>())
        {
            suitableConfigs.Add(expeditionConfig);
        }

        if (suitableConfigs.Count == 0)
        {
            Log.Error("Expedition mission generation failed: No suitable biomes.");
            QueueDel(generator);
            return;
        }

        var selectedConfig = _random.Pick(suitableConfigs);
        generator.Comp.DungeonConfig = selectedConfig;
    }

    private void SpawnMission(Entity<CP14ExpeditionGeneratorComponent> generator)
    {
        var cancelToken = new CancellationTokenSource();
        var job = new CP14SpawnExpeditionJob(
            JobMaxTime,
            EntityManager,
            _logManager,
            _mapManager,
            _proto,
            _biome,
            _dungeon,
            _metaData,
            _mapSystem,
            generator.Comp.DungeonConfig,
            _random.Next(-10000, 10000),
            cancelToken.Token);

        _expeditionJobs.Add((job, cancelToken));
        _expeditionQueue.EnqueueJob(job);
    }
}
