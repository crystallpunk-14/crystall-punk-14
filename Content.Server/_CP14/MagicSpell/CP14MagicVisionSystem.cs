using Content.Shared._CP14.MagicSpell;
using Content.Shared._CP14.MagicSpell.Components;
using Robust.Shared.Timing;

namespace Content.Server._CP14.MagicSpell;

public sealed class CP14MagicVisionSystem : CP14SharedMagicVisionSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<CP14MagicVisionFadeComponent>();
        while (query.MoveNext(out var uid, out var marker))
        {
            if (_timing.CurTime < marker.EndTime)
                continue;

            QueueDel(uid);
        }
    }
}
