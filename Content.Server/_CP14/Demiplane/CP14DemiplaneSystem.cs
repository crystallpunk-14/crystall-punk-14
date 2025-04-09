using Content.Server._CP14.Demiplane.Components;
using Content.Server._CP14.RoundStatistic;
using Content.Server.Chat.Managers;
using Content.Server.Chat.Systems;
using Content.Server.Flash;
using Content.Server.Procedural;
using Content.Shared._CP14.Demiplane;
using Content.Shared._CP14.Demiplane.Components;
using Content.Shared.Chat;
using Content.Shared.Popups;
using Robust.Server.Audio;
using Robust.Shared.Audio;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

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
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly CP14RoundStatTrackerSystem _statistic = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;

    private EntityQuery<CP14DemiplaneComponent> _demiplaneQuery;

    public override void Initialize()
    {
        base.Initialize();

        _demiplaneQuery = GetEntityQuery<CP14DemiplaneComponent>();

        InitGeneration();
        InitConnections();
        InitStabilization();
        InitEchoes();
        InitDestruction();

        SubscribeLocalEvent<CP14DemiplaneComponent, ComponentShutdown>(OnDemiplanShutdown);
        SubscribeLocalEvent<CP14SpawnOutOfDemiplaneComponent, MapInitEvent>(OnSpawnOutOfDemiplane);
    }

    private void OnSpawnOutOfDemiplane(Entity<CP14SpawnOutOfDemiplaneComponent> ent, ref MapInitEvent args)
    {
        //Check if entity is in demiplane
        var map = Transform(ent).MapUid;
        if (!_demiplaneQuery.TryComp(map, out var demiplane))
            return;

        //Get random exit demiplane point and spawn entity there
        if (demiplane.ExitPoints.Count == 0)
            return;

        var exit = _random.Pick(demiplane.ExitPoints);
        var coordinates = Transform(exit).Coordinates;

        var proto = ent.Comp.Proto;

        if (proto is null)
            proto = MetaData(ent).EntityPrototype?.ID;

        Spawn(proto, coordinates);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        UpdateGeneration(frameTime);
        UpdateStabilization(frameTime);
        UpdateDestruction(frameTime);
    }

    /// <summary>
    /// Teleports the entity inside the demiplane, to one of the random entry points.
    /// </summary>
    /// <param name="demiplane">The demiplane the entity will be teleported to</param>
    /// <param name="entity">The entity to be teleported</param>
    /// <returns></returns>
    public override bool TryTeleportIntoDemiplane(Entity<CP14DemiplaneComponent> demiplane, EntityUid? entity)
    {
        if (entity is null)
            return false;

        if (!TryGetDemiplaneEntryPoint(demiplane, out var entryPoint) || entryPoint is null)
        {
            Log.Error($"{entity} cant get in demiplane {demiplane}: no active entry points!");
            return false;
        }

        TeleportEntityToCoordinate(entity.Value, Transform(entryPoint.Value).Coordinates, demiplane.Comp.ArrivalSound);

        return true;
    }

    /// <summary>
    /// Simple teleportation, with common special effects for all the game's teleportation mechanics
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="coordinates"></param>
    /// <param name="sound"></param>
    public void TeleportEntityToCoordinate(EntityUid? entity, EntityCoordinates coordinates, SoundSpecifier? sound = null)
    {
        if (entity is null)
            return;

        _flash.Flash(entity.Value, null, null, 3000f, 0.5f);
        _transform.SetCoordinates(entity.Value, coordinates);
        _audio.PlayGlobal(sound, entity.Value);
    }

    /// <summary>
    /// Teleports an entity from the demiplane to the real world, to one of the random exit points in the real world.
    /// </summary>
    /// <param name="demiplane">The demiplane from which the entity will be teleported</param>
    /// <param name="entity">An entity that will be teleported into the real world. This entity must be in the demiplane, otherwise the function will not work.</param>
    /// <returns></returns>
    public override bool TryTeleportOutDemiplane(Entity<CP14DemiplaneComponent> demiplane, EntityUid? entity)
    {
        if (entity is null)
            return false;

        if (Transform(entity.Value).MapUid != demiplane.Owner)
            return false;

        if (!TryGetDemiplaneExitPoint(demiplane, out var connection) || connection is null)
        {
            Log.Error($"{entity} cant get out of demiplane {demiplane}: no active connections!");
            return false;
        }

        TeleportEntityToCoordinate(entity.Value, Transform(connection.Value).Coordinates, demiplane.Comp.DepartureSound);
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
            RemoveDemiplaneRandomEntryPoint(demiplane, entry);
        }
    }

    private void DemiplaneAnnounce(EntityUid mapUid, string text)
    {
        var mapId = Comp<MapComponent>(mapUid).MapId;

        _chatManager.ChatMessageToManyFiltered(
            Filter.BroadcastMap(mapId),
            ChatChannel.Radio,
            text,
            text,
            _mapManager.GetMapEntityId(mapId),
            false,
            true,
            null);
    }
}
