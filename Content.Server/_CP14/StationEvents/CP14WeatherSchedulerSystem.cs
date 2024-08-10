using Content.Server._CP14.StationEvents.Components;
using Content.Server.GameTicking.Rules;
using Content.Server.StationEvents;
using Content.Shared.GameTicking.Components;
using Robust.Shared.Random;

namespace Content.Server._CP14.StationEvents;

public sealed class CP14WeatherSchedulerSystem : GameRuleSystem<CP14WeatherSchedulerComponent>
{
    [Dependency] private readonly IRobustRandom _random = default!;

    protected override void Started(EntityUid uid,
        CP14WeatherSchedulerComponent component,
        GameRuleComponent gameRule,
        GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        component.NextEventTime = Timing.CurTime + TimeSpan.FromSeconds(component.Delays.Next(RobustRandom));
    }

    protected override void ActiveTick(EntityUid uid, CP14WeatherSchedulerComponent component, GameRuleComponent gameRule, float frameTime)
    {
        base.ActiveTick(uid, component, gameRule, frameTime);

        if (Timing.CurTime <= component.NextEventTime)
            return;

        component.NextEventTime = Timing.CurTime + TimeSpan.FromSeconds(component.Delays.Next(RobustRandom));

        GameTicker.StartGameRule(_random.Pick(component.Protos));
    }
}

