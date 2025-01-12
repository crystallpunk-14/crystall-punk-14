using Content.Shared._CP14.DayCycle;
using Content.Shared._CP14.DayCycle.Components;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._CP14.DayCycle;

public sealed partial class CP14DayCycleSystem : CP14SharedDayCycleSystem
{
    public const int MinTimeEntryCount = 2;
    private const float MaxTimeDiff = 0.05f;

    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14DayCycleComponent, MapInitEvent>(OnMapInitDayCycle);
    }

    private void OnMapInitDayCycle(Entity<CP14DayCycleComponent> dayCycle, ref MapInitEvent args)
    {
        dayCycle.Comp.IndexedCycle = _proto.Index(dayCycle.Comp.CycleProto);

        Init(dayCycle);

        if (dayCycle.Comp.StartWithRandomEntry && dayCycle.Comp.IndexedCycle.TimeEntries.Count > 1)
            SetTimeEntry(dayCycle, _random.Next(dayCycle.Comp.IndexedCycle.TimeEntries.Count));
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var dayCycleQuery = EntityQueryEnumerator<CP14DayCycleComponent, MapLightComponent>();
        while (dayCycleQuery.MoveNext(out var uid, out var dayCycle, out var mapLight))
        {
            var entity = new Entity<CP14DayCycleComponent, MapLightComponent>(uid, dayCycle, mapLight);

            if (dayCycle.IndexedCycle is null)
                continue;

            if (dayCycle.IndexedCycle.TimeEntries.Count < MinTimeEntryCount)
                continue;

            SetAmbientColor((entity, entity), GetCurrentColor(entity, _timing.CurTime.TotalSeconds));

            if (_timing.CurTime <= dayCycle.EntryEndTime)
                continue;

            SetTimeEntry((uid, dayCycle),  dayCycle.NextTimeEntryIndex);
        }
    }

    public void Init(Entity<CP14DayCycleComponent> dayCycle)
    {
        if (dayCycle.Comp.IndexedCycle is null)
            return;

        if (dayCycle.Comp.IndexedCycle.TimeEntries.Count < MinTimeEntryCount)
        {
            Log.Warning($"Attempting to init a day/night cycle with the number of time entries less than {MinTimeEntryCount}");
            return;
        }

        dayCycle.Comp.CurrentTimeEntryIndex = 0;
        dayCycle.Comp.EntryStartTime = _timing.CurTime;
        dayCycle.Comp.EntryEndTime = _timing.CurTime + dayCycle.Comp.CurrentTimeEntry!.Value.Duration;

        Dirty(dayCycle);
    }

    public void AddTimeEntry(Entity<CP14DayCycleComponent> dayCycle, DayCycleEntry entry)
    {
        if (dayCycle.Comp.IndexedCycle is null)
            return;

        dayCycle.Comp.IndexedCycle.TimeEntries.Add(entry);
        Dirty(dayCycle);
    }

    public void SetTimeEntry(Entity<CP14DayCycleComponent> dayCycle, int nextEntry)
    {
        if (dayCycle.Comp.IndexedCycle is null)
            return;

        nextEntry = Math.Clamp(nextEntry, 0, dayCycle.Comp.IndexedCycle.TimeEntries.Count - 1);

        dayCycle.Comp.CurrentTimeEntryIndex = nextEntry;
        dayCycle.Comp.EntryStartTime = dayCycle.Comp.EntryEndTime;
        dayCycle.Comp.EntryEndTime += dayCycle.Comp.CurrentTimeEntry!.Value.Duration;

        var ev = new DayCycleChangedEvent(dayCycle.Comp.CurrentTimeEntry);
        RaiseLocalEvent(dayCycle, ref ev, true);

        Dirty(dayCycle);
    }

    private void SetAmbientColor(Entity<MapLightComponent> light, Color color)
    {
        if (color == light.Comp.AmbientLightColor)
            return;

        light.Comp.AmbientLightColor = color;
        Dirty(light);
    }

    private Color GetCurrentColor(Entity<CP14DayCycleComponent> dayCycle, double totalSeconds)
    {
        var timeScale = GetTimeScale(dayCycle, totalSeconds);
        return Color.InterpolateBetween(dayCycle.Comp.StartColor ?? Color.Black, dayCycle.Comp.EndColor ?? Color.Black, timeScale);
    }

    private float GetTimeScale(Entity<CP14DayCycleComponent> dayCycle, double totalSeconds)
    {
        return GetLerpValue(dayCycle.Comp.EntryStartTime.TotalSeconds, dayCycle.Comp.EntryEndTime.TotalSeconds, totalSeconds);
    }

    private static float GetLerpValue(double start, double end, double current)
    {
        if (Math.Abs(start - end) < MaxTimeDiff)
            return 0f;

        var distanceFromStart = current - start;
        var totalDistance = end - start;

        return MathHelper.Clamp01((float)(distanceFromStart / totalDistance));
    }
}
