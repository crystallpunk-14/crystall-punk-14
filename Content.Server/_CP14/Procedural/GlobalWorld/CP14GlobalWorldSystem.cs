using Content.Server._CP14.Procedural.GlobalWorld.Components;
using Content.Server.Shuttles.Systems;
using Content.Server.Station.Events;
using Content.Server.Station.Systems;
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

    private ISawmill _sawmill = default!;

    public override void Initialize()
    {
        base.Initialize();

        _sawmill = _logManager.GetSawmill("cp14_global_world");

        SubscribeLocalEvent<CP14StationGlobalWorldComponent, StationPostInitEvent>(OnIntegratedPostInit);
    }

    private void OnIntegratedPostInit(Entity<CP14StationGlobalWorldComponent> ent, ref StationPostInitEvent args)
    {
        GenerateGlobalWorldMap(ent);

        foreach (var node in ent.Comp.Nodes)
        {
            if (node.Value.LocationConfig is null)
                continue;

            var mapId = node.Value.MapUid;
            if (mapId is null)
            {
                _map.CreateMap(out var newMapId, runMapInit: false);
                mapId = newMapId;
            }

            var mapUid = _map.GetMap(mapId.Value);
            _generation.GenerateLocation(mapUid, mapId.Value, node.Value.LocationConfig.Value, node.Value.Modifiers);

            //We dont wanna rename our settlement
            if (node.Key != Vector2i.Zero)
                _meta.SetEntityName(mapUid, $"{node.Key} - World {node.Value.LocationConfig.Value}");
        }
    }
}
