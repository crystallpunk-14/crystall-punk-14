using System.Linq;
using Content.Server.Weather;
using Content.Shared._CP14.WeatherControl;
using Content.Shared.Weather;
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

            var weatherData = PickWeatherDataByWeight(uid, weather.Entries);

            if (weatherData is null)
                continue;

            if (!_proto.TryIndex(weatherData.Visuals, out var weatherVisualsIndexed))
            {
                var weatherDuration = TimeSpan.FromSeconds(weatherData.Duration.Next(_random));
                _weather.SetWeather(Transform(uid).MapID, null, null);
                weather.NextWeatherTime = _timing.CurTime + weatherDuration;
            }
            else
            {
                var weatherDuration = TimeSpan.FromSeconds(weatherData.Duration.Next(_random));
                _weather.SetWeather(Transform(uid).MapID, weatherVisualsIndexed, null);
                weather.NextWeatherTime = _timing.CurTime + weatherDuration;
            }
        }
    }

    private CP14WeatherData? PickWeatherDataByWeight(EntityUid map, HashSet<CP14WeatherData> entries)
    {
        var filteredEntries = new HashSet<CP14WeatherData>(entries);

        if (TryComp<WeatherComponent>(map, out var currentWeather))
        {
            foreach (var (weatherProto, data) in currentWeather.Weather)
            {
                if (!_proto.TryIndex(weatherProto, out var weatherPrototype))
                    continue;

                filteredEntries.RemoveWhere(entry => entry.Visuals == weatherPrototype);
            }
        }

        if (filteredEntries.Count == 0)
            return null;

        var totalWeight = filteredEntries.Sum(entry => entry.Weight);
        var randomWeight = _random.NextFloat() * totalWeight;
        var currentWeight = 0f;

        foreach (var entry in filteredEntries)
        {
            currentWeight += entry.Weight;
            if (randomWeight < currentWeight)
            {
                return entry;
            }
        }

        // Fallback in case of rounding errors
        return filteredEntries.Last();
    }
}
