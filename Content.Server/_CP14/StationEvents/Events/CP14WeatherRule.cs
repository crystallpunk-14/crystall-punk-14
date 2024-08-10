using Content.Server._CP14.StationEvents.Components;
using Content.Server.StationEvents.Events;
using Content.Shared.GameTicking.Components;
using Content.Shared.Station.Components;
using Content.Shared.Weather;

namespace Content.Server._CP14.StationEvents.Events;

public sealed class CP14WeatherRule : StationEventSystem<CP14WeatherRuleComponent>
{
    [Dependency] private readonly SharedWeatherSystem _weather = default!;

    protected override void Started(EntityUid uid, CP14WeatherRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        if (!TryGetRandomStation(out var chosenStation))
            return;

        if (!PrototypeManager.TryIndex(component.Weather, out var weather))
            return;

        var query = EntityQueryEnumerator<StationMemberComponent, TransformComponent>();
        while (query.MoveNext(out uid, out var member, out var transformComponent))
        {
            if (member.Station != chosenStation)
                continue;

            _weather.SetWeather(transformComponent.MapID,  weather, null);
            component.Map = transformComponent.MapID;
            break;
        }
    }

    protected override void Ended(EntityUid uid, CP14WeatherRuleComponent component, GameRuleComponent gameRule, GameRuleEndedEvent args)
    {
        base.Ended(uid, component, gameRule, args);

        if (component.Map is not null)
            _weather.SetWeather(component.Map.Value, null, WeatherComponent.ShutdownTime);
    }
}
