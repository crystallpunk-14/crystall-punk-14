using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.StationEvents;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server.CrystallPunk.MapEvents;

public sealed class CPMapEventsSchedulerSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] public readonly GameTicker GameTicker = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CPMapEventsSchedulerComponent, MapInitEvent>(OnSchedulerInit);
    }

    private void OnSchedulerInit(Entity<CPMapEventsSchedulerComponent> scheduler, ref MapInitEvent args)
    {
        scheduler.Comp.NextEventTime = _timing.CurTime + TimeSpan.FromSeconds(scheduler.Comp.MinimumTimeUntilFirstEvent);
        scheduler.Comp.Map = Transform(scheduler).MapID;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<CPMapEventsSchedulerComponent>();
        while (query.MoveNext(out var uid, out var scheduler))
        {
            if (_timing.CurTime < scheduler.NextEventTime)
                continue;

            var randomEvent = _random.Pick(scheduler.WhitelistedEvents);

            //var ent = GameTicker.AddGameRule(randomEvent);
            GameTicker.StartGameRule(randomEvent);

            scheduler.NextEventTime = _timing.CurTime + TimeSpan.FromSeconds(_random.NextFloat(scheduler.MinimumTimeBetweenEvents, scheduler.MaximumTimeBetweenEvents));
        }
    }
}
