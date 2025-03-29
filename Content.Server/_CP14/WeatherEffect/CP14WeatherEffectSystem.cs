using System.Threading;
using Content.Shared._CP14.WeatherEffect;
using Content.Shared.Weather;
using Robust.Shared.CPUJob.JobQueues;
using Robust.Shared.CPUJob.JobQueues.Queues;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.WeatherEffect;

public sealed class CP14WeatherEffectSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;

    private readonly JobQueue _weatherQueue = new();
    private readonly List<(CP14WeatherEffectJob Job, CancellationTokenSource CancelToken)> _weatherJobs = new();
    private const double JobMaxTime = 0.002;

    public override void Initialize()
    {
        base.Initialize();


    }

    private void UpdateJob(float frameTime)
    {
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
    }


    private void ProcessWeather()
    {
        var query = EntityQueryEnumerator<WeatherComponent, MapComponent>();
        while (query.MoveNext(out var mapUid, out var weather, out var mapComp))
        {
            foreach (var (proto, _) in weather.Weather)
            {
                if (!_proto.TryIndex(proto, out var indexedWeather))
                    continue;

                foreach (var config in indexedWeather.Config)
                {

                    config.Effects
                }
            }
        }
    }
}
