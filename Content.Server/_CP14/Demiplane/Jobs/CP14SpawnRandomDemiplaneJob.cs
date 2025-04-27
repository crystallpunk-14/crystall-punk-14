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

public sealed class CP14SpawnRandomDemiplaneJob(
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
    CancellationToken cancellation = default) : Job<bool>(maxTime, cancellation)
{
    public readonly EntityUid DemiplaneMapUid = demiplaneMapUid;

    private readonly ISawmill _sawmill = logManager.GetSawmill("cp14_demiplane_job");

    protected override async Task<bool> Process()
    {
        _sawmill.Debug($"Spawning demiplane `{config.Id}` with seed {seed}");
        var gridComp = entManager.EnsureComponent<MapGridComponent>(DemiplaneMapUid);

        MetaDataComponent? metadata = null;
        DungeonConfigPrototype dungeonConfig = new();

        metaData.SetEntityName(DemiplaneMapUid, $"D:{config.Id} - {seed}");

        //Setup demiplane config
        var expeditionConfig = protoManager.Index(config);
        var indexedLocation = protoManager.Index(expeditionConfig.LocationConfig);

        dungeonConfig.Data = indexedLocation.Data;
        dungeonConfig.Layers.AddRange(indexedLocation.Layers);
        dungeonConfig.ReserveTiles = indexedLocation.ReserveTiles;

        //Add map components
        entManager.AddComponents(DemiplaneMapUid, expeditionConfig.Components);

        //Apply modifiers
        foreach (var modifier in modifiers)
        {
            if (!protoManager.TryIndex(modifier, out var indexedModifier))
                continue;

            if (indexedModifier.Layers != null)
                dungeonConfig.Layers.AddRange(indexedModifier.Layers);
            if (indexedModifier.Components != null)
                entManager.AddComponents(DemiplaneMapUid, indexedModifier.Components);

            _sawmill.Debug($"Added modifier: {seed} - {modifier.Id}");
        }

        //Setup gravity
        var gravity = entManager.EnsureComponent<GravityComponent>(DemiplaneMapUid);
        gravity.Enabled = true;
        entManager.Dirty(DemiplaneMapUid, gravity, metadata);

        // Setup default atmos
        var moles = new float[Atmospherics.AdjustedNumberOfGases];
        moles[(int) Gas.Oxygen] = 21.824779f;
        moles[(int) Gas.Nitrogen] = 82.10312f;
        var mixture = new GasMixture(moles, Atmospherics.T20C);
        entManager.System<AtmosphereSystem>().SetMapAtmosphere(DemiplaneMapUid, false, mixture);

        map.InitializeMap(demiplaneMapId);
        map.SetPaused(demiplaneMapId, false);

        //Spawn modified config
        dungeon.GenerateDungeon(dungeonConfig,
            DemiplaneMapUid,
            gridComp,
            Vector2i.Zero,
            seed); //TODO: Transform to Async

        return true;
    }
}
