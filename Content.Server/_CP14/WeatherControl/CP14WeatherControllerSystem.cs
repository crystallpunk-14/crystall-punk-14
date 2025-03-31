using Content.Server.Weather;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._CP14.WeatherControl;

public sealed class CP14WeatherControllerSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly WeatherSystem _weather = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14WeatherControllerComponent, MapInitEvent>(OnMapInit);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<CP14WeatherControllerComponent>();
        while (query.MoveNext(out var uid, out var weather))
        {
            if (_timing.CurTime <= weather.NextWeatherTime)
                continue;

            if (!weather.Enabled)
                continue;

            var weatherData = _random.Pick(weather.Entries);

            if (!_proto.TryIndex(weatherData.Visuals, out var weatherVisualsIndexed))
                continue;

            var weatherDuration = TimeSpan.FromSeconds(weatherData.Duration.Next(_random));
            _weather.SetWeather(Transform(uid).MapID, weatherVisualsIndexed, _timing.CurTime + weatherDuration);

            var clearDuration = TimeSpan.FromSeconds(weather.ClearDuration.Next(_random));
            weather.NextWeatherTime = _timing.CurTime + weatherDuration + clearDuration;
        }
    }

    private void OnMapInit(Entity<CP14WeatherControllerComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.NextWeatherTime = _timing.CurTime + TimeSpan.FromSeconds(ent.Comp.ClearDuration.Next(_random));
    }
}

