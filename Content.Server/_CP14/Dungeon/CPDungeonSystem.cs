using System.Threading;
using Content.Server.Procedural;
using Content.Server.Station.Systems;
using Content.Shared._CP14.Dungeon;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Teleportation.Systems;
using Robust.Server.Audio;
using Robust.Shared.CPUJob.JobQueues.Queues;
using Robust.Shared.Map;
using Robust.Shared.Physics.Events;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._CP14.Dungeon;

public sealed partial class CPDungeonSystem : EntitySystem
{
    [Dependency] private readonly LinkedEntitySystem _link = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IEntitySystemManager _entitySystem = default!;

    [Dependency] private readonly ILogManager _logManager = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly DungeonSystem _dungeon = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;



    private readonly JobQueue _dungeonGenQueue = new();
    private readonly List<(CPSpawnDungeonLevelJob Job, CancellationTokenSource CancelToken)> _dungeonGenJobs = new();
    private const double DungeonGenTime = 0.002;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CPDungeonEntranceComponent, ActivateInWorldEvent>(OnActivateInWorld);
    }

    private void OnActivateInWorld(Entity<CPDungeonEntranceComponent> entrance, ref ActivateInWorldEvent args)
    {
        var tempLevelParams = new CPDungeonLevelParams();
        tempLevelParams.Seed = _random.Next(0,1000);
        tempLevelParams.Depth = 0;
        tempLevelParams.DungeonConfig = "Haunted";
        SpawnDungeonLevel(tempLevelParams);
    }

    private void SpawnDungeonLevel(CPDungeonLevelParams levelParams)
    {
        var cancelToken = new CancellationTokenSource();
        var job = new CPSpawnDungeonLevelJob(
            DungeonGenTime,
            EntityManager,
            _logManager,
            _mapManager,
            _prototypeManager,
            _dungeon,
            _metaData,
            levelParams,
            cancelToken.Token);

        _dungeonGenJobs.Add((job, cancelToken));
        _dungeonGenQueue.EnqueueJob(job);
        _dungeonGenQueue.Process();
    }
}
