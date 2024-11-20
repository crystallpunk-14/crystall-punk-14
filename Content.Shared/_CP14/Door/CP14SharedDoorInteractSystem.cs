using Content.Shared.Interaction;
using Content.Shared.Interaction.Components;
using Content.Shared.Lock;

namespace Content.Shared._CP14.Door;

public sealed class CP14DoorInteractSystem : EntitySystem
{
    [Dependency] private readonly InteractionPopupSystem _interactPopup = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<LockComponent, InteractHandEvent>(OnInteractHand);
    }


    private void OnInteractHand(EntityUid uid, LockComponent component, InteractHandEvent args)
    {
        if (!component.Locked)
            return;

        if (!TryComp<InteractionPopupComponent>(args.Target, out var interactComponent))
            return;

        _interactPopup.SharedInteract(uid, interactComponent, args, args.Target, args.User);
    }

}
