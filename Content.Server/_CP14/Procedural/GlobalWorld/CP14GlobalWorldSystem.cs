using Content.Server._CP14.Procedural.GlobalWorld.Components;
using Content.Server.Shuttles.Systems;
using Content.Server.Station.Events;
using Content.Server.Station.Systems;
using Content.Shared.Teleportation.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._CP14.Procedural.GlobalWorld;

public sealed partial class CP14GlobalWorldSystem : EntitySystem
{
    [Dependency] private readonly ShuttleSystem _shuttles = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly ILogManager _logManager = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly CP14LocationGenerationSystem _generation = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly MetaDataSystem _meta = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly LinkedEntitySystem _link = default!;

    private ISawmill _sawmill = default!;

    public override void Initialize()
    {
        base.Initialize();

        _sawmill = _logManager.GetSawmill("cp14_global_world");

        SubscribeLocalEvent<CP14StationGlobalWorldComponent, StationPostInitEvent>(OnIntegratedPostInit);
        SubscribeLocalEvent<CP14LocationGeneratedEvent>(OnLocationGenerated);
        SubscribeLocalEvent<CP14StationGlobalWorldComponent, CP14GlobalWorldGeneratedEvent>(OnGlobalWorldGenerated);
    }

    private void OnLocationGenerated(CP14LocationGeneratedEvent args)
    {
        var query = EntityQueryEnumerator<CP14StationGlobalWorldComponent>();
        while (query.MoveNext(out var ent, out var comp))
        {
            //Theres no support for multiple GlobalWorld
            if (comp.LocationInGeneration.Contains(args.JobName))
            {
                comp.LocationInGeneration.Remove(args.JobName);
                _sawmill.Debug($"Location {args.JobName} generated successfully. Remaining: {comp.LocationInGeneration.Count}");
            }

            if (comp.LocationInGeneration.Count == 0)
            {
                //All locations are generated, we can now spawn the global world map
                _sawmill.Debug("All locations generated, spawning global world map.");
                var ev = new CP14GlobalWorldGeneratedEvent();
                RaiseLocalEvent(ent, ev);
            }
        }
    }

    private void OnIntegratedPostInit(Entity<CP14StationGlobalWorldComponent> ent, ref StationPostInitEvent args)
    {
        GenerateGlobalWorldMap(ent);
        SpawnGlobalWorldMap(ent);
    }

    private void OnGlobalWorldGenerated(Entity<CP14StationGlobalWorldComponent> ent, ref CP14GlobalWorldGeneratedEvent ev)
    {
        ConnectGlobalWorldMap(ent);
    }

    private void SpawnGlobalWorldMap(Entity<CP14StationGlobalWorldComponent> ent)
    {
        foreach (var (position, node) in ent.Comp.Nodes)
        {
            if (node.LocationConfig is null)
                continue;

            if (node.MapUid is null)
            {
                _map.CreateMap(out var newMapId, runMapInit: false);
                node.MapUid = newMapId;
            }

            var mapId = node.MapUid.Value;
            var mapUid = _map.GetMap(mapId);
            var jobName = $"job_GW_{position}";
            ent.Comp.LocationInGeneration.Add(jobName);

            _generation.GenerateLocation(
                mapUid,
                mapId,
                node.LocationConfig.Value,
                node.Modifiers,
                jobName: jobName
            );

            // Avoid renaming the settlement
            if (position != Vector2i.Zero)
            {
                var newName = $"{position} - {node.LocationConfig.Value}";
                _meta.SetEntityName(mapUid, newName);
            }
        }
    }

    private void ConnectGlobalWorldMap(Entity<CP14StationGlobalWorldComponent> ent)
    {
        foreach (var edge in ent.Comp.Edges)
        {
            var firstNodeMapUid = ent.Comp.Nodes[edge.Item1].MapUid;
            var secondNodeMapUid = ent.Comp.Nodes[edge.Item2].MapUid;

            EntityUid? firstConnector = null;
            EntityUid? secondConnector = null;

            //Get random connector from map
            var query = EntityQueryEnumerator<CP14GlobalWorldConnectorComponent>();
            while (query.MoveNext(out var uid, out _))
            {
                if (_transform.GetMapId(uid) == firstNodeMapUid)
                    firstConnector = uid;

                if (_transform.GetMapId(uid) == secondNodeMapUid)
                    secondConnector = uid;

            }

            if (firstConnector == null || secondConnector == null)
            {
                _sawmill.Error($"Failed to find connectors for edge {edge.Item1} - {edge.Item2}. " +
                               $"First: {firstConnector}, Second: {secondConnector}");
                continue;
            }

            var firstPortal = Spawn("CP14LocationPassway", Transform(firstConnector.Value).Coordinates);
            var secondPortal = Spawn("CP14LocationPassway", Transform(secondConnector.Value).Coordinates);

            _link.TryLink(firstPortal, secondPortal, true);

            Del(firstConnector);
            Del(secondConnector);
        }
    }
}

public sealed class CP14GlobalWorldGeneratedEvent : EntityEventArgs
{
}
