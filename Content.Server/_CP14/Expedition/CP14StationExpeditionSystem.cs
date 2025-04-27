using System.Threading;
using Content.Server._CP14.Demiplane.Jobs;
using Content.Server._CP14.RoundStatistic;
using Content.Server.Chat.Managers;
using Content.Server.Chat.Systems;
using Content.Server.Flash;
using Content.Server.Procedural;
using Content.Server.Shuttles.Systems;
using Content.Server.Station.Events;
using Content.Server.Station.Systems;
using Content.Shared.Popups;
using Robust.Server.Audio;
using Robust.Shared.CPUJob.JobQueues;
using Robust.Shared.CPUJob.JobQueues.Queues;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._CP14.Expedition;

public sealed class CP14StationExpeditionSystem : EntitySystem
{
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly MapLoaderSystem _loader = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly ShuttleSystem _shuttles = default!;
    [Dependency] private readonly StationSpawningSystem _stationSpawning = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly ILogManager _logManager = default!;
    [Dependency] private readonly DungeonSystem _dungeon = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly FlashSystem _flash = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly CP14RoundStatTrackerSystem _statistic = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;

    private const double JobMaxTime = 0.002;
    private readonly JobQueue _expeditionQueue = new();
    private readonly List<(CP14SpawnRandomDemiplaneJob Job, CancellationTokenSource CancelToken)> _expeditionJobs = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14StationExpeditionComponent, StationPostInitEvent>(StationInit);
    }
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        UpdateGeneration(frameTime);
    }

    private void UpdateGeneration(float frameTime)
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

    private void StationInit(Entity<CP14StationExpeditionComponent> ent, ref StationPostInitEvent args)
    {
        //First - spawn expedition map

        var expeditionMap = _mapSystem.CreateMap(out var mapId, false);
        ent.Comp.ExpeditionMap = expeditionMap;

        var cancelToken = new CancellationTokenSource();

        var job = new CP14SpawnRandomDemiplaneJob(
            JobMaxTime,
            EntityManager,
            _logManager,
            _proto,
            _dungeon,
            _metaData,
            _mapSystem,
            expeditionMap,
            mapId,
            ent.Comp.Location,
            ent.Comp.Modifiers,
            _random.Next(-10000, 10000),
            cancelToken.Token);

        _expeditionJobs.Add((job, cancelToken));
        _expeditionQueue.EnqueueJob(job);
    }
}
