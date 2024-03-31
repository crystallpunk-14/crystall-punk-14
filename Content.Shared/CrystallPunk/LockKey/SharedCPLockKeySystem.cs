
using Content.Shared.Containers.ItemSlots;
using Content.Shared.CrystallPunk.LockKey;
using Content.Shared.Doors;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Robust.Shared.Containers;
using Robust.Shared.Serialization;
using System.Diagnostics.CodeAnalysis;

namespace Content.Shared.CrystallPunk.LockKey;


public abstract class SharedCPLockKeySystem : EntitySystem
{

    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CPLockSlotComponent, ContainerIsInsertingAttemptEvent>(OnLockInsertAttempt);

        SubscribeLocalEvent<CPLockSlotComponent, EntInsertedIntoContainerMessage>(OnLockInserted);
        SubscribeLocalEvent<CPLockSlotComponent, EntRemovedFromContainerMessage>(OnLockRemoved);

        SubscribeLocalEvent<CPLockSlotComponent, BeforeDoorOpenedEvent>(OnBeforeDoorOpened);
        SubscribeLocalEvent<CPLockSlotComponent, ActivateInWorldEvent>(OnActivateInWorld);
    }

    private void OnActivateInWorld(EntityUid uid, CPLockSlotComponent component, ActivateInWorldEvent args)
    {
        if (args.Handled)
            return;

        if (!TryGetLockFromSlot(uid, out var lockEnt))
            return;

        if (lockEnt.Value.Comp.Locked)
        {
            _popup.PopupEntity(Loc.GetString("cp-lock-target-use-failed-locked", ("target", MetaData(uid).EntityName)), uid, args.User);
            args.Handled = true;
        }
    }
    private void OnBeforeDoorOpened(EntityUid uid, CPLockSlotComponent component, BeforeDoorOpenedEvent args)
    {
        if (!TryGetLockFromSlot(uid, out var lockEnt))
            return;

        if (lockEnt.Value.Comp.Locked)
        {
            if (args.User != null)
                _popup.PopupEntity(Loc.GetString("cp-lock-target-use-failed-locked", ("target", MetaData(uid).EntityName)), uid, args.User.Value);

            args.Cancel();
        }
    }

    private void OnLockInsertAttempt(Entity<CPLockSlotComponent> lockSlot, ref ContainerIsInsertingAttemptEvent args)
    {
        if (!lockSlot.Comp.Initialized)
            return;

        if (args.Container.ID != lockSlot.Comp.LockSlotId)
            return;

        if (!TryComp<CPLockComponent>(args.EntityUid, out var lockComp))
        {
            args.Cancel();
            return;
        }

        if (lockComp == null)
            return;

        if (lockComp.Locked)
        {
            _popup.PopupEntity(Loc.GetString(
                "cp-lock-lock-insert-fail-locked",
                ("lock", MetaData(args.EntityUid).EntityName),
                ("target", MetaData(lockSlot).EntityName)), lockSlot);
            args.Cancel();
        }
    }

    private void OnLockInserted(Entity<CPLockSlotComponent> lockSlot, ref EntInsertedIntoContainerMessage args)
    {
        if (!lockSlot.Comp.Initialized)
            return;

        if (args.Container.ID != lockSlot.Comp.LockSlotId)
            return;

        if (!TryComp<CPLockComponent>(args.Entity, out var lockComp))
            return;

        LockLock(new Entity<CPLockComponent>(args.Entity, lockComp));
        _appearance.SetData(lockSlot, LockSlotVisuals.LockExist, true);
    }

    private void OnLockRemoved(Entity<CPLockSlotComponent> lockSlot, ref EntRemovedFromContainerMessage args)
    {
        if (args.Container.ID != lockSlot.Comp.LockSlotId)
            return;
        _appearance.SetData(lockSlot, LockSlotVisuals.LockExist, false);
    }
    public abstract bool TryGetLockFromSlot(EntityUid uid,
    [NotNullWhen(true)] out Entity<CPLockComponent>? lockEnt,
    CPLockSlotComponent? component = null);
    public abstract bool TryUseKeyOnLock(EntityUid user, Entity<CPKeyComponent> keyEnt, Entity<CPLockComponent> lockEnt);
    public abstract void LockLock(Entity<CPLockComponent> lockEnt);
    public abstract void UnlockLock(Entity<CPLockComponent> lockEnt);
}

[Serializable, NetSerializable]
public enum LockSlotVisuals : byte
{
    LockExist
}
