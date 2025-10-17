using Content.Shared._CP14.MagicVision;
using Robust.Shared.Timing;
using Content.Shared.StatusEffectNew;
using Robust.Shared.Prototypes;


namespace Content.Server._CP14.MagicVision;

public sealed class CP14MagicVisionSystem : CP14SharedMagicVisionSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedEyeSystem _eye = default!;
    [Dependency] private readonly StatusEffectsSystem _status = default!;

    private static EntProtoId _magicalVisionId = "CP14MetaMagicVisionSpellStatusEffect";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MetaDataComponent, CP14MagicVisionToggleActionEvent>(OnMagicVisionToggle);
        SubscribeLocalEvent<CP14MagicVisionStatusEffectComponent, GetVisMaskEvent>(OnGetVisMaskBody);
        SubscribeLocalEvent<CP14MagicVisionStatusEffectComponent, StatusEffectRelayedEvent<GetVisMaskEvent>>(OnGetVisMask);


        SubscribeLocalEvent<CP14MagicVisionStatusEffectComponent, StatusEffectAppliedEvent>(OnApplied);
        SubscribeLocalEvent<CP14MagicVisionStatusEffectComponent, StatusEffectRemovedEvent>(OnRemoved);


    }

    private void OnGetVisMask(Entity<CP14MagicVisionStatusEffectComponent> ent, ref StatusEffectRelayedEvent<GetVisMaskEvent> args)
    {
        var eventArgs = args.Args;
        var appliedMask = (int)CP14MagicVisionStatusEffectComponent.VisibilityMask;

        eventArgs.VisibilityMask |= appliedMask;
    }
    private void OnGetVisMaskBody(Entity<CP14MagicVisionStatusEffectComponent> ent, ref GetVisMaskEvent args)
    {
        var appliedMask = (int)CP14MagicVisionStatusEffectComponent.VisibilityMask;

        args.VisibilityMask |= appliedMask;
    }
    private void OnMagicVisionToggle(Entity<MetaDataComponent> ent, ref CP14MagicVisionToggleActionEvent args)
    {
        if (!_status.CanAddStatusEffect(ent, _magicalVisionId))
            return;

        if (_status.HasStatusEffect(ent, _magicalVisionId))
            _status.TryRemoveStatusEffect(ent, _magicalVisionId);
        else
            _status.TrySetStatusEffectDuration(ent, _magicalVisionId);
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
        _eye.RefreshVisibilityMask(ent.Owner);
    }

    private void OnRemoved(Entity<CP14MagicVisionStatusEffectComponent> ent, ref StatusEffectRemovedEvent args)
    {
        _eye.RefreshVisibilityMask(ent.Owner);
    }
}
