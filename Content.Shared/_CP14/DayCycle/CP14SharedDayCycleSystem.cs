using Content.Shared._CP14.DayCycle.Components;
using Content.Shared._CP14.DayCycle.Prototypes;
using Content.Shared.Storage.Components;
using Content.Shared.Weather;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.DayCycle;

public abstract class CP14SharedDayCycleSystem : EntitySystem
{
    private static readonly ProtoId<CP14DayCyclePeriodPrototype> DayPeriod = "Day";

    [Dependency] private readonly SharedMapSystem _maps = default!;
    [Dependency] private readonly SharedWeatherSystem _weather = default!;

    private EntityQuery<MapGridComponent> _mapGridQuery;

    public override void Initialize()
    {
        base.Initialize();

        _mapGridQuery = GetEntityQuery<MapGridComponent>();
    }

    /// <summary>
    /// Checks to see if the specified entity is on the map where it's daytime.
    /// </summary>
    /// <param name="target">An entity being tested to see if it is in daylight</param>
    /// <param name="checkRoof">Checks if the tile covers the weather (the only "roof" factor at the moment)</param>
    public bool UnderSunlight(EntityUid target)
    {
        if (HasComp<InsideEntityStorageComponent>(target))
            return false;

        var xform = Transform(target);
        if (!TryComp<CP14DayCycleComponent>(xform.MapUid, out var dayCycle))
            return false;

        var day = dayCycle.CurrentPeriod == DayPeriod;

        if (!TryComp<MapGridComponent>(xform.GridUid, out var mapGrid))
            return day;

        var grid = xform.GridUid;
        if (grid is null)
            return day;

        if (!_mapGridQuery.TryComp(grid, out var gridComp))
            return day;

        if (!_weather.CanWeatherAffect(grid.Value, gridComp, _maps.GetTileRef(xform.GridUid.Value, mapGrid, xform.Coordinates)))
            return false;

        return day;
    }
}
