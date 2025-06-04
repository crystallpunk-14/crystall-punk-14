using System.Threading;
using System.Threading.Tasks;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Procedural;
using Content.Shared._CP14.Demiplane.Prototypes;
using Content.Shared.Atmos;
using Content.Shared.Gravity;
using Content.Shared.Procedural;
using Robust.Shared.CPUJob.JobQueues;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.Demiplane.Jobs;

public sealed class CP14SpawnRandomDemiplaneJob : Job<bool>
{
    private readonly IEntityManager _entManager;
    //private readonly IGameTiming _timing;
    private readonly IPrototypeManager _prototypeManager;
    //private readonly AnchorableSystem _anchorable;
    private readonly DungeonSystem _dungeon;
    private readonly MetaDataSystem _metaData;
    //private readonly SharedTransformSystem _xforms;
    private readonly SharedMapSystem _map;

    private readonly ProtoId<CP14DemiplaneLocationPrototype> _config;
    private readonly List<ProtoId<CP14DemiplaneModifierPrototype>> _modifiers;
    private readonly int _seed;

    public readonly EntityUid DemiplaneMapUid;
    private readonly MapId _demiplaneMapId;

    private readonly ISawmill _sawmill;

    public CP14SpawnRandomDemiplaneJob(
        double maxTime,
        IEntityManager entManager,
        ILogManager logManager,
        IPrototypeManager protoManager,
        DungeonSystem dungeon,
        MetaDataSystem metaData,
        SharedMapSystem map,
        EntityUid demiplaneMapUid,
        MapId demiplaneMapId,
        ProtoId<CP14DemiplaneLocationPrototype> config,
        List<ProtoId<CP14DemiplaneModifierPrototype>> modifiers,
        int seed,
        CancellationToken cancellation = default) : base(maxTime, cancellation)
    {
        _entManager = entManager;
        _prototypeManager = protoManager;
        _dungeon = dungeon;
        _metaData = metaData;
        _map = map;
        DemiplaneMapUid = demiplaneMapUid;
        _demiplaneMapId = demiplaneMapId;
        _config = config;
        _modifiers = modifiers;
        _seed = seed;

        _sawmill = logManager.GetSawmill("cp14_demiplane_job");
    }

    protected override async Task<bool> Process()
    {
        _sawmill.Debug($"Spawning demiplane `{_config.Id}` with seed {_seed}");
        var gridComp = _entManager.EnsureComponent<MapGridComponent>(DemiplaneMapUid);

        MetaDataComponent? metadata = null;
        DungeonConfigPrototype dungeonConfig = new();

        _metaData.SetEntityName(DemiplaneMapUid, $"Demiplane {_config.Id} - {_seed}");

        //Setup demiplane config
        var expeditionConfig = _prototypeManager.Index(_config);
        var indexedLocation = _prototypeManager.Index(expeditionConfig.LocationConfig);

        dungeonConfig.Layers.AddRange(indexedLocation.Layers);
        dungeonConfig.ReserveTiles = indexedLocation.ReserveTiles;

        //Add map components
        _entManager.AddComponents(DemiplaneMapUid, expeditionConfig.Components);

        //Apply modifiers
        foreach (var modifier in _modifiers)
        {
            if (!_prototypeManager.TryIndex(modifier, out var indexedModifier))
                continue;

            if (indexedModifier.Layers != null)
                dungeonConfig.Layers.AddRange(indexedModifier.Layers);
            if (indexedModifier.Components != null)
                _entManager.AddComponents(DemiplaneMapUid, indexedModifier.Components);

            _sawmill.Debug($"Added modifier: {_seed} - {modifier.Id}");
        }

        //Setup gravity
        var gravity = _entManager.EnsureComponent<GravityComponent>(DemiplaneMapUid);
        gravity.Enabled = true;
        _entManager.Dirty(DemiplaneMapUid, gravity, metadata);

        // Setup default atmos
        var moles = new float[Atmospherics.AdjustedNumberOfGases];
        moles[(int) Gas.Oxygen] = 21.824779f;
        moles[(int) Gas.Nitrogen] = 82.10312f;
        var mixture = new GasMixture(moles, Atmospherics.T20C);
        _entManager.System<AtmosphereSystem>().SetMapAtmosphere(DemiplaneMapUid, false, mixture);

        _map.InitializeMap(_demiplaneMapId);
        _map.SetPaused(_demiplaneMapId, false);

        //Spawn modified config
        _dungeon.GenerateDungeon(dungeonConfig,
            DemiplaneMapUid,
            gridComp,
            Vector2i.Zero,
            _seed); //TODO: Transform to Async

        return true;
    }
}
