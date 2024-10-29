using System.Threading;
using Content.Server._CP14.Expeditions.Components;
using Content.Server._CP14.Expeditions.Jobs;
using Content.Server.Parallax;
using Content.Shared._CP14.Expeditions;
using Content.Shared._CP14.Expeditions.Prototypes;
using Content.Shared.Interaction.Events;
using Content.Shared.Parallax.Biomes;
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
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly IGameTiming _timing = default!;


    private readonly JobQueue _expeditionQueue = new();
    private readonly List<(CP14SpawnExpeditionJob Job, CancellationTokenSource CancelToken)> _expeditionJobs = new();
    private const double JobMaxTime = 0.002;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14ExpeditionGeneratorComponent, UseInHandEvent>(GeneratorUsedInHand);
        SubscribeLocalEvent<CP14ExpeditionComponent, MapInitEvent>(OnExpeditionInit);
        SubscribeLocalEvent<CP14ExpeditionComponent, ComponentShutdown>(OnExpeditionShutdown);
    }

    private void OnExpeditionShutdown(Entity<CP14ExpeditionComponent> ent, ref ComponentShutdown args)
    {
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
        var newParams = GenerateMissionParams(generator);

        if (newParams is not null)
        {
            generator.Comp.MissionParams = newParams;
            Log.Debug($"New mission params seed: {newParams.Seed}");
            Log.Debug($"New mission params biome: {newParams.Biome}");

            SpawnMission(generator);
        }
    }

    private CP14ExpeditionMissionParams? GenerateMissionParams(Entity<CP14ExpeditionGeneratorComponent> generator)
    {
        CP14ExpeditionMissionParams missionParams = new();

        //Seed
        missionParams.Seed = _random.Next(-10000, 10000);

        //Biome
        HashSet<ProtoId<BiomeTemplatePrototype>> suitableBiomes = new();
        foreach (var biomePrototype in _proto.EnumeratePrototypes<CP14ExpeditionsBiomePrototype>())
        {
            suitableBiomes.Add(biomePrototype.Biome);
        }

        if (suitableBiomes.Count == 0)
        {
            Log.Error("Expedition mission generation failed: No suitable biomes.");
            return null;
        }
        missionParams.Biome = _random.Pick(suitableBiomes);

        //
        return missionParams;
    }

    private void SpawnMission(Entity<CP14ExpeditionGeneratorComponent> generator)
    {
        if (generator.Comp.MissionParams is null)
        {
            Log.Error("Expedition mission generation failed: Mission params is null.");
            return;
        }

        var cancelToken = new CancellationTokenSource();
        var job = new CP14SpawnExpeditionJob(
            JobMaxTime,
            EntityManager,
            _logManager,
            _mapManager,
            _proto,
            _biome,
            _metaData,
            _mapSystem,
            generator.Comp.MissionParams,
            cancelToken.Token);

        _expeditionJobs.Add((job, cancelToken));
        _expeditionQueue.EnqueueJob(job);
    }
}
