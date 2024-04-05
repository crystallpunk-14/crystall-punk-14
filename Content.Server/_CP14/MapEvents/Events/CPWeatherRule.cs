using Content.Server.GameTicking.Rules.Components;
using Content.Server.Station.Systems;
using Content.Server.StationEvents.Events;
using Content.Server.Weather;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server.CrystallPunk.MapEvents;


public sealed class CPWeatherRule : StationEventSystem<CPWeatherRuleComponent>
{

    [Dependency] private readonly WeatherSystem _weather = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    protected override void Started(EntityUid uid, CPWeatherRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);


        var mapId = GameTicker.DefaultMap;
        var duration = _random.NextFloat(component.MinimumLength, component.MaximumLength);

        _weather.SetWeather(mapId, _proto.Index(component.Weather), _timing.CurTime + TimeSpan.FromSeconds(duration));
    }
}
