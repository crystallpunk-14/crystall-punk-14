using Content.Server.Flash;
using Content.Server.Procedural;
using Content.Shared._CP14.Demiplane;
using Content.Shared._CP14.Demiplane.Components;
using Robust.Server.Audio;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._CP14.Demiplane;

public sealed partial class CP14DemiplaneSystem : CP14SharedDemiplaneSystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ILogManager _logManager = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly DungeonSystem _dungeon = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly FlashSystem _flash = default!;

    public override void Initialize()
    {
        base.Initialize();

        InitGeneration();
        InitConnections();

        SubscribeLocalEvent<CP14DemiplaneComponent, ComponentShutdown>(OnDemiplanShutdown);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        UpdateGeneration(frameTime);
    }

    /// <summary>
    /// Teleports the entity inside the demiplane, to one of the random entry points.
    /// </summary>
    /// <param name="demiplane">The demiplane the entity will be teleported to</param>
    /// <param name="entity">The entity to be teleported</param>
    /// <returns></returns>
    public bool TryTeleportIntoDemiplane(Entity<CP14DemiplaneComponent> demiplane, EntityUid entity)
    {
        if (!TryGetDemiplanEntryPoint(demiplane, out var entryPoint) || entryPoint is null)
        {
            Log.Error($"{entity} cant get in demiplane {demiplane}: no active entry points!");
            return false;
        }

        var targetCoord = Transform(entryPoint.Value).Coordinates;
        _flash.Flash(entity, null, null, 2f, 0.5f);
        _transform.SetCoordinates(entity, targetCoord);
        _audio.PlayGlobal(demiplane.Comp.ArrivalSound, entity);

        return true;
    }

    /// <summary>
    /// Teleports an entity from the demiplane to the real world, to one of the random exit points in the real world.
    /// </summary>
    /// <param name="demiplan">The demiplane from which the entity will be teleported</param>
    /// <param name="entity">An entity that will be teleported into the real world. This entity must be in the demiplane, otherwise the function will not work.</param>
    /// <returns></returns>
    public bool TryTeleportOutDemiplane(Entity<CP14DemiplaneComponent> demiplan, EntityUid entity)
    {
        if (Transform(entity).MapUid != demiplan.Owner)
            return false;

        if (!TryGetDemiplanExitPoint(demiplan, out var connection) || connection is null)
        {
            Log.Error($"{entity} cant get out of demiplane {demiplan}: no active connections!");
            return false;
        }

        var targetCoord = Transform(connection.Value).Coordinates;
        _flash.Flash(entity, null, null, 2f, 0.5f);
        _transform.SetCoordinates(entity, targetCoord);
        _audio.PlayGlobal(demiplan.Comp.DepartureSound, entity);

        return true;
    }

    private void OnDemiplanShutdown(Entity<CP14DemiplaneComponent> demiplane, ref ComponentShutdown args)
    {
        //We stop asynchronous generation of a demiplane early if for some reason this demiplane is deleted before generation is complete
        foreach (var (job, cancelToken) in _expeditionJobs.ToArray())
        {
            if (job.DemiplaneMapUid == demiplane.Owner)
            {
                cancelToken.Cancel();
                _expeditionJobs.Remove((job, cancelToken));
            }
        }

        foreach (var exit in demiplane.Comp.ExitPoints)
        {
            RemoveDemiplanRandomExitPoint(demiplane, exit);
        }

        foreach (var entry in demiplane.Comp.EntryPoints)
        {
            RemoveDemiplanRandomEntryPoint(demiplane, entry);
        }
    }
}
