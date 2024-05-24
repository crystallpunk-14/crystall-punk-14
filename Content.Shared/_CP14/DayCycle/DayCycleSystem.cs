using Robust.Shared.Map.Components;
using Robust.Shared.Timing;

namespace Content.Shared._CP14.DayCycle;

public sealed partial class DayCycleSystem : EntitySystem
{
    public const int MinTimeEntryCount = 2;
    public const float MaxTimeDiff = 0.05f;

    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DayCycleComponent, MapInitEvent>(OnMapInitDayCycle);
        SubscribeLocalEvent<DayCycleComponent, DayCycleDayStartedEvent>(OnDayStarted);
        SubscribeLocalEvent<DayCycleComponent, DayCycleNightStartedEvent>(OnNightStarted);
    }

    private void OnDayStarted(Entity<DayCycleComponent> dayCycle, ref DayCycleDayStartedEvent args)
    {
    }

    private void OnNightStarted(Entity<DayCycleComponent> dayCycle, ref DayCycleNightStartedEvent args)
    {
    }

    private void OnMapInitDayCycle(Entity<DayCycleComponent> dayCycle, ref MapInitEvent args)
    {
        if (dayCycle.Comp.TimeEntries.Count < MinTimeEntryCount)
        {
            Log.Warning("Attempting to create a daily cycle with the number of time entries less than 2");
            return;
        }

        var currentEntry = dayCycle.Comp.TimeEntries[0];
        dayCycle.Comp.EntryStartTime = _timing.CurTime;
        dayCycle.Comp.EntryEndTime = _timing.CurTime + currentEntry.Duration;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var dayCycleQuery = EntityQueryEnumerator<DayCycleComponent, MapLightComponent>();
        while (dayCycleQuery.MoveNext(out var uid, out var dayCycle, out var mapLight))
        {
            var entity = new Entity<DayCycleComponent, MapLightComponent>(uid, dayCycle, mapLight);

            if (dayCycle.TimeEntries.Count < MinTimeEntryCount)
                continue;

            var color = GetCurrentColor(entity, _timing.CurTime.TotalSeconds);
            if (color != mapLight.AmbientLightColor)
            {
                mapLight.AmbientLightColor = color;
                Dirty(uid, mapLight);
            }

            if (_timing.CurTime <= dayCycle.EntryEndTime)
                continue;

            SetTimeEntry((uid, dayCycle),  dayCycle.NextTimeEntry);
        }
    }

    public void SetTimeEntry(Entity<DayCycleComponent> dayCycle, int nextEntry)
    {
        nextEntry = Math.Clamp(nextEntry, 0, dayCycle.Comp.TimeEntries.Count - 1);

        dayCycle.Comp.CurrentTimeEntry = nextEntry;
        dayCycle.Comp.EntryStartTime = dayCycle.Comp.EntryEndTime;
        dayCycle.Comp.EntryEndTime += dayCycle.Comp.TimeEntries[nextEntry].Duration;

        // TODO: Made with states,we might need an evening or something, and besides, it's too much hardcore
        if (dayCycle.Comp.IsNight && !dayCycle.Comp.TimeEntries[nextEntry].IsNight) // Day started
        {
            dayCycle.Comp.IsNight = false;

            var ev = new DayCycleDayStartedEvent(dayCycle);
            RaiseLocalEvent(dayCycle, ref ev, true);
        }

        if (!dayCycle.Comp.IsNight && dayCycle.Comp.TimeEntries[nextEntry].IsNight) // Night started
        {
            dayCycle.Comp.IsNight = true;

            var ev = new DayCycleNightStartedEvent(dayCycle);
            RaiseLocalEvent(dayCycle, ref ev, true);
        }
    }

    private Color GetCurrentColor(Entity<DayCycleComponent> dayCycle, double totalSeconds)
    {
        var timeScale = GetTimeScale(dayCycle, totalSeconds);
        return Color.InterpolateBetween(dayCycle.Comp.StartColor, dayCycle.Comp.EndColor, timeScale);
    }

    private float GetTimeScale(Entity<DayCycleComponent> dayCycle, double totalSeconds)
    {
        return GetLerpValue(dayCycle.Comp.EntryStartTime.TotalSeconds, dayCycle.Comp.EntryEndTime.TotalSeconds, totalSeconds);
    }

    public static float GetLerpValue(double start, double end, double current)
    {
        if (Math.Abs(start - end) < MaxTimeDiff)
            return 0f;

        var distanceFromStart = current - start;
        var totalDistance = end - start;

        return MathHelper.Clamp01((float)(distanceFromStart / totalDistance));
    }
}
