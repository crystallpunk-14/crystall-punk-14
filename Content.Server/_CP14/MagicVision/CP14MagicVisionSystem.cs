using Content.Shared._CP14.MagicVision;
using Content.Shared.Eye;
using Robust.Shared.Timing;
using Content.Shared.StatusEffectNew;
using Robust.Shared.Prototypes;


namespace Content.Server._CP14.MagicVision;

public sealed class CP14MagicVisionSystem : CP14SharedMagicVisionSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedEyeSystem _eye = default!;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MetaDataComponent, CP14MagicVisionToggleActionEvent>(OnMagicVisionToggle);
        SubscribeLocalEvent<CP14MagicVisionStatusEffectComponent, GetVisMaskEvent>(OnGetVisMask);

        SubscribeLocalEvent<CP14MagicVisionStatusEffectComponent, StatusEffectAppliedEvent>(OnApplied);
        SubscribeLocalEvent<CP14MagicVisionStatusEffectComponent, StatusEffectRemovedEvent>(OnRemoved);


    }

    private void OnGetVisMask(Entity<CP14MagicVisionStatusEffectComponent> ent, ref GetVisMaskEvent args)
    {
        if (ent.Comp.Overlay)
            args.VisibilityMask |= (int)CP14MagicVisionStatusEffectComponent.VisibilityMask;
    }

    private void OnMagicVisionToggle(Entity<MetaDataComponent> ent, ref CP14MagicVisionToggleActionEvent args)
    {
        if (!HasComp<CP14MagicVisionStatusEffectComponent>(ent))
        {
            AddComp<CP14MagicVisionStatusEffectComponent>(ent);
        }
        else
        {
            RemComp<CP14MagicVisionStatusEffectComponent>(ent);
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

        private void OnApplied(Entity<CP14MagicVisionStatusEffectComponent> ent, ref StatusEffectAppliedEvent args)
    {
        if (!ent.Comp.Overlay)
            return;

        if (TryComp<EyeComponent>(ent.Owner, out var comp))
        {
            var appliedMask = (int)CP14MagicVisionStatusEffectComponent.VisibilityMask;

            // Apply the visibility mask if it's not already present
/*             if ((comp.VisibilityMask & appliedMask) == 0)
                comp.VisibilityMask |= appliedMask; */
        }

    }

    private void OnRemoved(Entity<CP14MagicVisionStatusEffectComponent> ent, ref StatusEffectRemovedEvent args)
    {
        if (TryComp<EyeComponent>(ent.Owner, out var comp))
        {
            var appliedMask = (int)CP14MagicVisionStatusEffectComponent.VisibilityMask;

            // Removes the visibility mask if it's present
/*             if ((comp.VisibilityMask & appliedMask) != 0)
                comp.VisibilityMask ^= appliedMask;
 */        }
    }

}
