using System.Threading;
using System.Threading.Tasks;
using Content.Server._CP14.Expeditions.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Parallax;
using Content.Shared._CP14.Expeditions;
using Content.Shared.Atmos;
using Content.Shared.Gravity;
using Content.Shared.Parallax.Biomes;
using Robust.Shared.CPUJob.JobQueues;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.Expeditions.Jobs;

public sealed class CP14SpawnExpeditionJob : Job<bool>
{
    private readonly IEntityManager _entManager;
    //private readonly IGameTiming _timing;
    private readonly IMapManager _mapManager;
    private readonly IPrototypeManager _prototypeManager;
    //private readonly AnchorableSystem _anchorable;
    private readonly BiomeSystem _biome;
    //private readonly DungeonSystem _dungeon;
    private readonly MetaDataSystem _metaData;
    //private readonly SharedTransformSystem _xforms;
    private readonly SharedMapSystem _map;

    public readonly CP14ExpeditionMissionParams MissionParams;

    private readonly ISawmill _sawmill;

    public CP14SpawnExpeditionJob(
        double maxTime,
        IEntityManager entManager,
        ILogManager logManager,
        IMapManager mapManager,
        IPrototypeManager protoManager,
        BiomeSystem biome,
        MetaDataSystem metaData,
        SharedMapSystem map,
        CP14ExpeditionMissionParams missionParams,
        CancellationToken cancellation = default) : base(maxTime, cancellation)
    {
        _entManager = entManager;
        _mapManager = mapManager;
        _prototypeManager = protoManager;
        _biome = biome;
        _metaData = metaData;
        _map = map;
        MissionParams = missionParams;
        _sawmill = logManager.GetSawmill("cp14_expedition_job");
    }

    protected override async Task<bool> Process()
    {
        _sawmill.Debug("cp14_expedition", $"Spawning expedition mission with seed {0}");
        var mapUid = _map.CreateMap(out var mapId, runMapInit: false);
        MetaDataComponent? metadata = null;
        var grid = _entManager.EnsureComponent<MapGridComponent>(mapUid);
        var random = new Random(0);
        _metaData.SetEntityName(mapUid, "TODO: Expedition name generation");

        //var difficultyId = "Moderate";
        //var difficultyProto = _prototypeManager.Index<SalvageDifficultyPrototype>(difficultyId);

        //Setup biome
        var missionBiome = _prototypeManager.Index(MissionParams.Biome);
        var biome = _entManager.AddComponent<BiomeComponent>(mapUid);
        var biomeSystem = _entManager.System<BiomeSystem>();
        biomeSystem.SetTemplate(mapUid, biome, missionBiome);
        biomeSystem.SetSeed(mapUid, biome, MissionParams.Seed);
        _entManager.Dirty(mapUid, biome);

        //Setup gravity
        var gravity = _entManager.EnsureComponent<GravityComponent>(mapUid);
        gravity.Enabled = true;
        _entManager.Dirty(mapUid, gravity, metadata);

        // Setup default atmos
        var moles = new float[Atmospherics.AdjustedNumberOfGases];
        moles[(int) Gas.Oxygen] = 21.824779f;
        moles[(int) Gas.Nitrogen] = 82.10312f;
        var mixture = new GasMixture(moles, Atmospherics.T20C);
        _entManager.System<AtmosphereSystem>().SetMapAtmosphere(mapUid, false, mixture);

        _mapManager.DoMapInitialize(mapId);
        _mapManager.SetMapPaused(mapId, false);

        //Setup expedition
        var expedition = _entManager.AddComponent<CP14ExpeditionComponent>(mapUid);
        expedition.MissionParams = MissionParams;

        //Dungeon

        //Guaranteed loot

        //Mobs spawn

        return true;
    }
}
