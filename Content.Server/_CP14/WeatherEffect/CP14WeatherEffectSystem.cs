using System.Threading;
using Content.Shared.Weather;
using Robust.Shared.CPUJob.JobQueues;
using Robust.Shared.CPUJob.JobQueues.Queues;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._CP14.WeatherEffect;

public sealed class CP14WeatherEffectSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedWeatherSystem _weather = default!;
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private readonly JobQueue _weatherQueue = new();
    private readonly List<(CP14WeatherEffectJob Job, CancellationTokenSource CancelToken)> _weatherJobs = new();
    private const double JobMaxTime = 0.002;

    private EntityQuery<BlockWeatherComponent> _weatherBlockQuery;
    public override void Initialize()
    {
        base.Initialize();

        _weatherBlockQuery = GetEntityQuery<BlockWeatherComponent>();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        _weatherQueue.Process();

        foreach (var (job, cancelToken) in _weatherJobs.ToArray())
        {
            switch (job.Status)
            {
                case JobStatus.Finished:
                    _weatherJobs.Remove((job, cancelToken));
                    break;
            }
        }

        ProcessWeather();
    }

    private void ProcessWeather()
    {
        var query = EntityQueryEnumerator<WeatherComponent, MapGridComponent>();
        while (query.MoveNext(out var mapUid, out var weather, out var mapComp))
        {
            foreach (var (proto, _) in weather.Weather)
            {
                if (!_proto.TryIndex(proto, out var indexedWeather))
                    continue;

                foreach (var config in indexedWeather.Config)
                {
                    if (_timing.CurTime <= config.NextEffectTime)
                        continue;

                    config.NextEffectTime = _timing.CurTime + config.Frequency;

                    var cancelToken = new CancellationTokenSource();
                    var job = new CP14WeatherEffectJob(
                        JobMaxTime,
                        EntityManager,
                        _lookup,
                        _weather,
                        _mapSystem,
                        _random,
                        (mapUid, mapComp),
                        Transform(mapUid).MapID,
                        config,
                        _weatherBlockQuery);

                    _weatherJobs.Add((job, cancelToken));
                    _weatherQueue.EnqueueJob(job);
                }
            }
        }
    }
}
