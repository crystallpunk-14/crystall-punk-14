using Content.Shared.GameTicking;
using Content.Shared.Light.Components;
using Content.Shared.Storage.Components;
using Content.Shared.Weather;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Timing;

namespace Content.Shared._CP14.DayCycle;

/// <summary>
/// This is an add-on to the LightCycle system that helps you determine what time of day it is on the map
/// </summary>
public sealed class CP14DayCycleSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly SharedGameTicker _ticker = default!;
    [Dependency] private readonly SharedMapSystem _maps = default!;
    [Dependency] private readonly SharedWeatherSystem _weather = default!;

    private EntityQuery<MapGridComponent> _mapGridQuery;
    private EntityQuery<InsideEntityStorageComponent> _storageQuery;


    public override void Initialize()
    {
        base.Initialize();

        _mapGridQuery = GetEntityQuery<MapGridComponent>();
        _storageQuery = GetEntityQuery<InsideEntityStorageComponent>();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<LightCycleComponent, CP14DayCycleComponent, MapComponent>();
        while (query.MoveNext(out var uid, out var lightCycle, out var dayCycle, out var map))
        {
            var oldLightLevel = dayCycle.LastLightLevel;
            var newLightLevel = GetLightLevel((uid, lightCycle));

            // Going into darkness
            if (oldLightLevel > newLightLevel)
            {
                if (oldLightLevel > dayCycle.Threshold)
                {
                    if (newLightLevel < dayCycle.Threshold)
                    {
                        var ev = new CP14StartNightEvent(uid);
                        RaiseLocalEvent(ev);
                    }
                }
            }

            // Going into light
            if (oldLightLevel < newLightLevel)
            {
                if (oldLightLevel < dayCycle.Threshold)
                {
                    if (newLightLevel > dayCycle.Threshold)
                    {
                        var ev = new CP14StartDayEvent(uid);
                        RaiseLocalEvent(ev);
                    }
                }
            }

            dayCycle.LastLightLevel = newLightLevel;
        }
    }

    public float GetLightLevel(Entity<LightCycleComponent?> map)
    {
        if (!Resolve(map.Owner, ref map.Comp, false))
            return 0;

        var time = (float)_timing.CurTime
            .Add(map.Comp.Offset)
            .Subtract(_ticker.RoundStartTimeSpan)
            .Subtract(_metaData.GetPauseTime(map))
            .TotalSeconds;

        var normalizedTime = time % map.Comp.Duration.TotalSeconds;
        var lightLevel = Math.Sin((normalizedTime / map.Comp.Duration.TotalSeconds) * MathF.PI);
        return (float)lightLevel;
    }

    /// <summary>
    /// Checks to see if the specified entity is on the map where it's daytime.
    /// </summary>
    /// <param name="target">An entity being tested to see if it is in daylight</param>
    public bool UnderSunlight(EntityUid target)
    {
        if (_storageQuery.HasComp(target))
            return false;

        var xform = Transform(target);

        if (xform.MapUid is null || xform.GridUid is null)
            return false;

        var day = GetLightLevel(xform.MapUid.Value) > 0.5f;

        var grid = xform.GridUid;
        if (grid is null)
            return day;

        if (!_mapGridQuery.TryComp(grid, out var gridComp))
            return day;

        if (!_weather.CanWeatherAffect(grid.Value,
                gridComp,
                _maps.GetTileRef(xform.GridUid.Value, gridComp, xform.Coordinates)))
            return false;

        return day;
    }
}

public sealed class CP14StartNightEvent(EntityUid map) : EntityEventArgs
{
    public EntityUid Map = map;
}

public sealed class CP14StartDayEvent(EntityUid map) : EntityEventArgs
{
    public EntityUid Map = map;
}
