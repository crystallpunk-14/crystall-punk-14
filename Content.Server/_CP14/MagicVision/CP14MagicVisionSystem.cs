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

    private static EntProtoId _magicalVisionSpellProtoId = "CP14MetaMagicVisionSpellStatusEffect";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MetaDataComponent, CP14MagicVisionToggleActionEvent>(OnMagicVisionSpellToggle);
        //SubscribeLocalEvent<CP14MagicVisionComponent, GetVisMaskEvent>(OnGetVisMaskBody); //Incase a non Status effect component
        SubscribeLocalEvent<EyeComponent, GetVisMaskEvent>(OnGetVisMask);

        SubscribeLocalEvent<CP14MagicVisionStatusEffectComponent, StatusEffectAppliedEvent>(OnApplied);
        SubscribeLocalEvent<CP14MagicVisionStatusEffectComponent, StatusEffectRemovedEvent>(OnRemoved);
    }

    private void OnGetVisMask(Entity<EyeComponent> ent, ref GetVisMaskEvent args)
    {
        if (!_status.HasEffectComp<CP14MagicVisionStatusEffectComponent>(ent))
            return;

        var appliedMask = (int)CP14MagicVisionStatusEffectComponent.VisibilityMask;

        args.VisibilityMask |= appliedMask;
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
