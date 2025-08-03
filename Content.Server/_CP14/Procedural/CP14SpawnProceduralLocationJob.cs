using System.Threading;
using System.Threading.Tasks;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Procedural;
using Content.Shared._CP14.Procedural.Prototypes;
using Content.Shared.Atmos;
using Content.Shared.Gravity;
using Content.Shared.Procedural;
using Content.Shared.Procedural.DungeonLayers;
using Robust.Shared.CPUJob.JobQueues;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.Procedural;

public sealed class CP14SpawnProceduralLocationJob(
    double maxTime,
    IEntityManager entManager,
    ILogManager logManager,
    IPrototypeManager protoManager,
    DungeonSystem dungeon,
    SharedMapSystem map,
    EntityUid mapUid,
    MapId mapId,
    Vector2i position,
    int seed,
    ProtoId<CP14ProceduralLocationPrototype> config,
    List<ProtoId<CP14ProceduralModifierPrototype>> modifiers,
    string? jobName = null,
    CancellationToken cancellation = default)
    : Job<bool>(maxTime, cancellation)
{
    public readonly EntityUid MapUid = mapUid;
    public string? JobName = jobName;

    private readonly ISawmill _sawmill = logManager.GetSawmill("cp14_procedural_location_job");

    protected override async Task<bool> Process()
    {
        _sawmill.Debug($"Spawning procedural location `{config.Id}` with seed {seed}");
        var gridComp = entManager.EnsureComponent<MapGridComponent>(MapUid);

        MetaDataComponent? metadata = null;
        DungeonConfigPrototype dungeonConfig = new();

        //Boilerplate: reserve all old grid tiles
        dungeonConfig.Layers.Add(new CP14ReserveGrid());

        //Setup location config
        var locationConfig = protoManager.Index(config);
        var indexedLocation = protoManager.Index(locationConfig.LocationConfig);

        dungeonConfig.Layers.AddRange(indexedLocation.Layers);
        dungeonConfig.ReserveTiles = indexedLocation.ReserveTiles;

        //Add map components
        entManager.AddComponents(MapUid, locationConfig.Components);

        //Apply modifiers
        foreach (var modifier in modifiers)
        {
            if (!protoManager.TryIndex(modifier, out var indexedModifier))
                continue;

            if (indexedModifier.Layers != null)
                dungeonConfig.Layers.AddRange(indexedModifier.Layers);
            if (indexedModifier.Components != null)
                entManager.AddComponents(MapUid, indexedModifier.Components);

            _sawmill.Debug($"Added modifier: {seed} - {modifier.Id}");
        }

        //Setup gravity
        var gravity = entManager.EnsureComponent<GravityComponent>(MapUid);
        gravity.Enabled = true;
        entManager.Dirty(MapUid, gravity, metadata);

        // Setup default atmos
        var moles = new float[Atmospherics.AdjustedNumberOfGases];
        moles[(int) Gas.Oxygen] = 21.824779f;
        moles[(int) Gas.Nitrogen] = 82.10312f;
        var mixture = new GasMixture(moles, Atmospherics.T20C);
        entManager.System<AtmosphereSystem>().SetMapAtmosphere(MapUid, false, mixture);

        if (!map.IsInitialized(mapId))
            map.InitializeMap(mapId);
        map.SetPaused(mapId, false);

        //Spawn modified config
        await dungeon.GenerateDungeonAsync(dungeonConfig,
            MapUid,
            gridComp,
            position,
            seed);

        return true;
    }
}
