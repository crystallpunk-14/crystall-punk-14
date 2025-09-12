using Content.Shared._CP14.Fishing.Components;
using Robust.Shared.Random;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;

namespace Content.Shared._CP14.Fishing;

public abstract class CP14SharedFishingSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var curTime = _gameTiming.CurTime;
        var query = EntityQueryEnumerator<CP14FishingRodComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            if (curTime <= component.FishingTime)
                continue;

            if (component.FishingFloat is null)
                continue;

            component.FishingTime += curTime;
            UpdateFishingFrequency(component);
        }
    }

    private void UpdateFishingFrequency(CP14FishingRodComponent component)
    {
        _random.SetSeed((int)_gameTiming.CurTick.Value);
        component.FishingTime += TimeSpan.FromSeconds(_random.NextDouble(component.MinAwaitTime, component.MaxAwaitTime));
    }

    [Serializable, NetSerializable]
    public sealed class FishingReelKeyMessage : EntityEventArgs
    {
        public EntityUid User { get; }
        public bool Reeling { get; }

        public FishingReelKeyMessage(EntityUid user, bool reeling)
        {
            User = user;
            Reeling = reeling;
        }
    }
}
