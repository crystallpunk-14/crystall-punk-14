using Content.Shared.Interaction;
using Content.Shared.Lock;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;

namespace Content.Shared._CP14.Door;

public sealed class CP14DoorInteractionPopupSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    private EntityQuery<LockComponent> _lockQuery;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CP14DoorInteractionPopupComponent, InteractHandEvent>(OnInteractHand);
    }


    private void OnInteractHand(EntityUid uid, CP14DoorInteractionPopupComponent component, InteractHandEvent args)
    {

        if (_lockQuery.TryComp(args.Target, out var lockComponent) && !lockComponent.Locked)
            return;

        var curTime = _gameTiming.CurTime;

        if (curTime < component.LastInteractTime + component.InteractDelay)
            return;

        _popupSystem.PopupEntity(Loc.GetString(component.InteractString), args.Target);
        _audio.PlayPvs(component.InteractSound, args.Target);

        component.LastInteractTime = curTime;
    }

}
