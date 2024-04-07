
using System.Threading;
using System.Threading.Tasks;
using Content.Server.Procedural;
using Content.Shared._CP14.Dungeon;
using Robust.Shared.CPUJob.JobQueues;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.Dungeon;

public sealed class CPSpawnDungeonLevelJob : Job<bool>
{
    private readonly IEntityManager _entManager;
    private readonly IMapManager _mapManager;
    private readonly IPrototypeManager _prototypeManager;
    private readonly DungeonSystem _dungeon;
    private readonly MetaDataSystem _metaData;
    private readonly CPDungeonLevelParams _levelParams;

    private readonly ISawmill _sawmill;

    public CPSpawnDungeonLevelJob(
        double maxTime,
        IEntityManager entManager,
        ILogManager logManager,
        IMapManager mapManager,
        IPrototypeManager protoManager,
        DungeonSystem dungeon,
        MetaDataSystem metaData,
        CPDungeonLevelParams levelParams,
        CancellationToken cancellation = default) : base(maxTime, cancellation)
    {
        _entManager = entManager;
        _mapManager = mapManager;
        _prototypeManager = protoManager;
        _metaData = metaData;
        _dungeon = dungeon;
        _levelParams = levelParams;
        _sawmill = logManager.GetSawmill("dungeonGen");
    }

    protected override async Task<bool> Process()
    {
        //Init empty map
        _sawmill.Debug($"Spawning new dungeon level with seed {_levelParams.Seed}. Depth: {_levelParams.Depth}");
        var mapId = _mapManager.CreateMap();
        var mapUid = _mapManager.GetMapEntityId(mapId);
        _mapManager.AddUninitializedMap(mapId);
        MetaDataComponent? metadata = null;
        var grid = _entManager.EnsureComponent<MapGridComponent>(mapUid);
        var random = new Random(_levelParams.Seed);
        _metaData.SetEntityName(mapUid,$"MapId: {mapId}, Depth: {_levelParams.Depth}");

        //Spawn dungeon
        var config = _prototypeManager.Index(_levelParams.DungeonConfig);
        var dungeon = await WaitAsyncTask(_dungeon.GenerateDungeonAsync(config, mapUid, grid,
            Vector2i.Zero, _levelParams.Seed));

        // Aborty
        if (dungeon.Rooms.Count == 0)
        {
            return false;
        }

        return true;
    }
}

