using Content.Server._CP14.GameTicking.Rules.Components;
using Content.Server._CP14.WeatherControl;
using Content.Server.GameTicking.Rules;
using Content.Server.Station.Components;
using Content.Server.Station.Systems;
using Content.Server.StationEvents.Events;
using Content.Server.Weather;
using Content.Shared.GameTicking.Components;
using Content.Shared.Station.Components;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server._CP14.GameTicking.Rules;

public sealed class CP14WeatherRule : StationEventSystem<CP14WeatherRuleComponent>
{
    [Dependency] private readonly WeatherSystem _weather = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    protected override void Started(EntityUid uid, CP14WeatherRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        var query = EntityQueryEnumerator<MapComponent, BecomesStationComponent>();
        while (query.MoveNext(out var mapUid, out var map, out var station))
        {
            if (!_proto.TryIndex(component.Weather, out var indexedWeather))
                continue;

            if (TryComp<CP14WeatherControllerComponent>(mapUid, out var controller))
            {
                controller.Enabled = false;
            }

            _weather.SetWeather(map.MapId, indexedWeather, null);
        }
    }

    protected override void Ended(EntityUid uid, CP14WeatherRuleComponent component, GameRuleComponent gameRule, GameRuleEndedEvent args)
    {
        base.Ended(uid, component, gameRule, args);

        var query = EntityQueryEnumerator<MapComponent, BecomesStationComponent>();
        while (query.MoveNext(out var mapUid, out var map, out var station))
        {
            if (TryComp<CP14WeatherControllerComponent>(mapUid, out var controller))
            {
                controller.NextWeatherTime = _timing.CurTime + TimeSpan.FromSeconds(controller.ClearDuration.Max);
                controller.Enabled = true;
            }

            _weather.SetWeather(map.MapId, null, null);
        }
    }
}
