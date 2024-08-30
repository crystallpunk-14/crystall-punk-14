using Content.Server.GameTicking;
using Content.Server.GameTicking.Events;
using Content.Server.Weather;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._CP14.WeatherControl;

public sealed class CP14WeatherControllerSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly WeatherSystem _weather = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14WeatherControllerComponent, RoundStartingEvent>(OnRoundStart);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<CP14WeatherControllerComponent>();
        while (query.MoveNext(out var uid, out var weather))
        {
            if (_timing.CurTime <= weather.NextEventTime)
                continue;

            weather.NextEventTime = _timing.CurTime + TimeSpan.FromSeconds(weather.Delays.Next(_random));

            var weatherProto = _random.Pick(weather.Protos);
            if (!_proto.TryIndex(weatherProto, out var weatherIndexed))
                continue;

            _weather.SetWeather(Transform(uid).MapID, weatherIndexed, _timing.CurTime + TimeSpan.FromSeconds(45));
        }
    }

    private void OnRoundStart(Entity<CP14WeatherControllerComponent> ent, ref RoundStartingEvent args)
    {
        ent.Comp.NextEventTime = _timing.CurTime + TimeSpan.FromSeconds(ent.Comp.Delays.Next(_random));
    }
}

