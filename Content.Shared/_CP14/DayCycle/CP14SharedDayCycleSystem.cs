using Content.Shared._CP14.DayCycle.Components;
using Content.Shared.Maps;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.DayCycle;

public abstract class CP14SharedDayCycleSystem : EntitySystem
{
    private static readonly ProtoId<CP14DayCyclePeriodPrototype> DayPeriod = "Day";

    [Dependency] private readonly SharedMapSystem _maps = default!;
    [Dependency] private readonly ITileDefinitionManager _tileDefManager = default!;

    /// <summary>
    /// Checks to see if the specified entity is on the map where it's daytime.
    /// </summary>
    /// <param name="target">An entity being tested to see if it is in daylight</param>
    /// <param name="checkRoof">Checks if the tile covers the weather (the only "roof" factor at the moment)</param>
    public bool TryDaylightThere(EntityUid target, bool checkRoof)
    {
        var xform = Transform(target);
        if (!TryComp<CP14DayCycleComponent>(xform.MapUid, out var dayCycle))
            return false;

        if (!checkRoof || !TryComp<MapGridComponent>(xform.GridUid, out var mapGrid))
            return dayCycle.CurrentPeriod == DayPeriod;

        var tileRef = _maps.GetTileRef(xform.GridUid.Value, mapGrid, xform.Coordinates);
        var tileDef = (ContentTileDefinition) _tileDefManager[tileRef.Tile.TypeId];

        if (!tileDef.Weather)
            return false;

        return dayCycle.CurrentPeriod == DayPeriod;
    }
}
