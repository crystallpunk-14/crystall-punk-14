using Content.Server._CP14.Procedural.GlobalWorld.Components;
using Content.Server.Shuttles.Systems;
using Content.Server.Station.Components;
using Content.Server.Station.Events;
using Content.Server.Station.Systems;

namespace Content.Server._CP14.Procedural.GlobalWorld;

public sealed class CP14GlobalWorldSystem : EntitySystem
{
    [Dependency] private readonly ShuttleSystem _shuttles = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly ILogManager _logManager = default!;
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly CP14LocationGenerationSystem _generation = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    private ISawmill _sawmill = default!;
    public override void Initialize()
    {
        base.Initialize();

        _sawmill = _logManager.GetSawmill("cp14_global_world");

        SubscribeLocalEvent<CP14StationGlobalWorldIntegratedComponent, StationPostInitEvent>(OnIntegratedPostInit);
    }

    private void OnIntegratedPostInit(Entity<CP14StationGlobalWorldIntegratedComponent> ent, ref StationPostInitEvent args)
    {
        if (!TryComp<StationDataComponent>(ent, out var stationData))
        {
            _sawmill.Error($"Station {ent} does not have a StationDataComponent.");
            return;
        }

        var largestStationGrid = _station.GetLargestGrid(stationData);

        if (largestStationGrid is null)
        {
            _sawmill.Error($"Station {ent} does not have a grid.");
            return;
        }

        var mapId = _transform.GetMapId(largestStationGrid.Value);
        _generation.GenerateLocation(largestStationGrid.Value, mapId, ent.Comp.Location, ent.Comp.Modifiers, ent.Comp.GenerationOffset);
    }
}
