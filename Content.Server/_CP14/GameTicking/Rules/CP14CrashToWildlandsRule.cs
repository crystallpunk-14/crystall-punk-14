using System.Linq;
using System.Numerics;
using Content.Server._CP14.GameTicking.Rules.Components;
using Content.Server._CP14.Procedural;
using Content.Server.GameTicking.Rules;
using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Systems;
using Content.Server.Station.Components;
using Content.Server.Station.Systems;
using Content.Shared.GameTicking.Components;
using Robust.Shared.Map;

namespace Content.Server._CP14.GameTicking.Rules;

public sealed class CP14ExpeditionToWindlandsRule : GameRuleSystem<CP14ExpeditionToWindlandsRuleComponent>
{
    [Dependency] private readonly ShuttleSystem _shuttles = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly ILogManager _logManager = default!;
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly CP14LocationGenerationSystem _generation = default!;

    private ISawmill _sawmill = default!;

    public override void Initialize()
    {
        base.Initialize();
        _sawmill = _logManager.GetSawmill("cp14_crash_to_windlands_rule");
    }

    protected override void Started(EntityUid uid,
        CP14ExpeditionToWindlandsRuleComponent component,
        GameRuleComponent gameRule,
        GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        var station = _station.GetStations().First();
        if (!TryComp<StationDataComponent>(station, out var stationData))
        {
            _sawmill.Error($"Station {station} does not have a StationDataComponent.");
            return;
        }

        var largestStationGrid = _station.GetLargestGrid(stationData);

        if (largestStationGrid is null)
        {
            _sawmill.Error($"Station {station} does not have a grid.");
            return;
        }

        EnsureComp<ShuttleComponent>(largestStationGrid.Value, out var shuttleComp);

        var windlands = _mapSystem.CreateMap(out var mapId, runMapInit: false);

        _generation.GenerateLocation(windlands, mapId, component.Location, component.Modifiers);
        _shuttles.FTLToCoordinates(largestStationGrid.Value, shuttleComp, new EntityCoordinates(windlands, Vector2.Zero), 0f, 0f, component.FloatingTime);
    }
}
