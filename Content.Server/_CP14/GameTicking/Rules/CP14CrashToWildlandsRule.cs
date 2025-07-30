using System.Linq;
using System.Numerics;
using System.Threading;
using Content.Server._CP14.Demiplane.Jobs;
using Content.Server._CP14.GameTicking.Rules.Components;
using Content.Server.Chat.Managers;
using Content.Server.Chat.Systems;
using Content.Server.Flash;
using Content.Server.GameTicking.Rules;
using Content.Server.Procedural;
using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Systems;
using Content.Server.Station.Components;
using Content.Server.Station.Systems;
using Content.Shared.GameTicking.Components;
using Content.Shared.Popups;
using Robust.Shared.CPUJob.JobQueues;
using Robust.Shared.CPUJob.JobQueues.Queues;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._CP14.GameTicking.Rules;

public sealed class CP14CrashToWindlandsRule : GameRuleSystem<CP14CrashToWindlandsRuleComponent>
{
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly MapLoaderSystem _loader = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly ShuttleSystem _shuttles = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly ILogManager _logManager = default!;
    [Dependency] private readonly DungeonSystem _dungeon = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly FlashSystem _flash = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;

    private ISawmill _sawmill = default!;

    private const double JobMaxTime = 0.002;
    private readonly JobQueue _expeditionQueue = new();
    private readonly List<(CP14SpawnRandomDemiplaneJob Job, CancellationTokenSource CancelToken)> _expeditionJobs = new();

    public override void Initialize()
    {
        base.Initialize();


        _sawmill = _logManager.GetSawmill("cp14_crash_to_windlands_rule");
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

    protected override void Started(EntityUid uid,
        CP14CrashToWindlandsRuleComponent component,
        GameRuleComponent gameRule,
        GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        var station = _station.GetStations().First();
        if (!TryComp<StationDataComponent>(station, out var stationData))
        {
            _sawmill.Error($"Station {station} does not have a StationDataComponent.");
            return;
        }

        var largestStationGrid = _station.GetLargestGrid(stationData);

        if (largestStationGrid is null)
        {
            _sawmill.Error($"Station {station} does not have a grid.");
            return;
        }

        EnsureComp<ShuttleComponent>(largestStationGrid.Value, out var shuttleComp);

        var windlands = CreateWindLands((uid, component));

        _shuttles.FTLToCoordinates(largestStationGrid.Value, shuttleComp, new EntityCoordinates(windlands, Vector2.Zero), 0f, 0f, 60f);
    }

    private EntityUid CreateWindLands(Entity<CP14CrashToWindlandsRuleComponent> ent)
    {
        var expeditionMap = _mapSystem.CreateMap(out var mapId, false);

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

        return expeditionMap;
    }
}
