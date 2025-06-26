using Content.Shared._CP14.MagicVision;
using Content.Shared.Eye;
using Robust.Shared.Timing;

namespace Content.Server._CP14.MagicVision;

public sealed class CP14MagicVisionSystem : CP14SharedMagicVisionSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedEyeSystem _eye = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MetaDataComponent, CP14MagicVisionToggleActionEvent>(OnMagicVisionToggle);
        SubscribeLocalEvent<CP14MagicVisionComponent, GetVisMaskEvent>(OnGetVisMask);
    }

    private void OnGetVisMask(Entity<CP14MagicVisionComponent> ent, ref GetVisMaskEvent args)
    {
        args.VisibilityMask |= (int)VisibilityFlags.CP14MagicVision;
    }

    private void OnMagicVisionToggle(Entity<MetaDataComponent> ent, ref CP14MagicVisionToggleActionEvent args)
    {
        if (!HasComp<CP14MagicVisionComponent>(ent))
        {
            AddComp<CP14MagicVisionComponent>(ent);
        }
        else
        {
            RemComp<CP14MagicVisionComponent>(ent);
        }
        _eye.RefreshVisibilityMask(ent.Owner);
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
