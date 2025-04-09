using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Content.Shared._CP14.WeatherEffect;
using Content.Shared.Weather;
using Robust.Shared.CPUJob.JobQueues;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Random;

namespace Content.Server._CP14.WeatherEffect;

public sealed class CP14WeatherEffectJob : Job<bool>
{
    private readonly IEntityManager _entManager;
    private readonly EntityLookupSystem _lookup;
    private readonly SharedWeatherSystem _weather;
    private readonly SharedMapSystem _mapSystem;
    private readonly IRobustRandom _random;

    private readonly Entity<MapGridComponent> _mapUid;
    private readonly MapId _mapId;

    private readonly CP14WeatherEffectConfig _config;

    private EntityQuery<BlockWeatherComponent> _weatherBlockQuery;

    private readonly HashSet<Entity<TransformComponent>> _entitiesOnMap = new();
    private List<Entity<TransformComponent>> _affectedEntities = new();

    public CP14WeatherEffectJob(
        double maxTime,
        IEntityManager entManager,
        EntityLookupSystem lookup,
        SharedWeatherSystem weather,
        SharedMapSystem mapSystem,
        IRobustRandom random,
        Entity<MapGridComponent> mapUid,
        MapId mapId,
        CP14WeatherEffectConfig config,
        EntityQuery<BlockWeatherComponent> weatherBlockQuery,
        CancellationToken cancellation = default
    ) : base(maxTime, cancellation)
    {
        _entManager = entManager;
        _lookup = lookup;
        _weather = weather;
        _mapSystem = mapSystem;
        _random = random;

        _mapUid = mapUid;
        _mapId = mapId;

        _config = config;
        _weatherBlockQuery = weatherBlockQuery;
    }

    protected override async Task<bool> Process()
    {
        _affectedEntities.Clear();
        _entitiesOnMap.Clear();
        _lookup.GetEntitiesOnMap(_mapId, _entitiesOnMap);

        //Calculate all affected entities by weather
        foreach (var ent in _entitiesOnMap)
        {
            //All weatherblocker entites should be affected by weather
            if (_config.CanAffectOnWeatherBlocker && _weatherBlockQuery.HasComp(ent))
            {
                _affectedEntities.Add(ent);
                continue;
            }

            //All entities on weathered tile should be affected by weather
            var tileRef = _mapSystem.GetTileRef(_mapUid, _mapUid.Comp, ent.Comp.Coordinates);
            if (_weather.CanWeatherAffect(_mapUid, _mapUid.Comp, tileRef))
            {
                _affectedEntities.Add(ent);
                continue;
            }
        }

        _random.Shuffle(_affectedEntities);

        // Limit the number of affected entities if specified
        if (_config.MaxEntities.HasValue && _affectedEntities.Count > _config.MaxEntities.Value)
        {
            _affectedEntities = _affectedEntities.Take(_config.MaxEntities.Value).ToList();
        }

        //Apply weather effects to affected entities
        foreach (var entity in _affectedEntities)
        {
            foreach (var effect in _config.Effects)
            {
                effect.ApplyEffect(_entManager, _random, entity);
            }
        }

        return true;
    }
}
