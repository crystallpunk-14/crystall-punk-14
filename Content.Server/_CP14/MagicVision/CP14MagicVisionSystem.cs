using Content.Shared._CP14.MagicVision;
using Robust.Shared.Timing;
using Content.Shared.StatusEffectNew;

namespace Content.Server._CP14.MagicVision;

public sealed class CP14MagicVisionSystem : CP14SharedMagicVisionSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedEyeSystem _eye = default!;
    [Dependency] private readonly StatusEffectsSystem _status = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14MagicVisionStatusEffectComponent, StatusEffectRelayedEvent<GetVisMaskEvent>>(OnGetVisMask);

        SubscribeLocalEvent<CP14MagicVisionStatusEffectComponent, StatusEffectAppliedEvent>(OnApplied);
        SubscribeLocalEvent<CP14MagicVisionStatusEffectComponent, StatusEffectRemovedEvent>(OnRemoved);
    }

    private void OnGetVisMask(Entity<CP14MagicVisionStatusEffectComponent> ent, ref StatusEffectRelayedEvent<GetVisMaskEvent> args)
    {
        var appliedMask = (int)CP14MagicVisionStatusEffectComponent.VisibilityMask;
        var newArgs = args.Args;

        newArgs.VisibilityMask |= appliedMask;
        args = args with { Args = newArgs };
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

    private void OnApplied(Entity<CP14MagicVisionStatusEffectComponent> ent, ref StatusEffectAppliedEvent args)
    {
        _eye.RefreshVisibilityMask(args.Target);
    }

    private void OnRemoved(Entity<CP14MagicVisionStatusEffectComponent> ent, ref StatusEffectRemovedEvent args)
    {
        _eye.RefreshVisibilityMask(args.Target);
    }
}
