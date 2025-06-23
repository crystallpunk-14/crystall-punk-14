using Content.Shared._CP14.MagicVision;
using Robust.Shared.Timing;

namespace Content.Server._CP14.MagicVision;

public sealed class CP14MagicVisionSystem : CP14SharedMagicVisionSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<CP14MagicVisionMarkerComponent>();
        while (query.MoveNext(out var uid, out var marker))
        {
            if (marker.EndTime == TimeSpan.Zero)
                continue;

            if (_timing.CurTime < marker.EndTime)
                continue;

            QueueDel(uid);
        }
    }
}
