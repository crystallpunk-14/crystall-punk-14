
using Content.Shared.Containers.ItemSlots;
using Content.Shared.CrystallPunk.LockKey;
using Content.Shared.Examine;
using Content.Shared.Popups;
using Robust.Shared.Containers;
using Robust.Shared.Serialization;

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
        SubscribeLocalEvent<CPLockSlotComponent, ContainerIsRemovingAttemptEvent>(OnLockRemovingAttempt);

        SubscribeLocalEvent<CPLockSlotComponent, EntInsertedIntoContainerMessage>(OnLockInserted);
        SubscribeLocalEvent<CPLockSlotComponent, EntRemovedFromContainerMessage>(OnLockRemoved);
    }


    private void OnLockRemovingAttempt(Entity<CPLockSlotComponent> lockSlot, ref ContainerIsRemovingAttemptEvent args)
    {
        if (!lockSlot.Comp.Initialized)
            return;

        TryComp<CPLockComponent>(args.EntityUid, out var lockComp);

        if (lockComp != null && lockComp.Locked)
        {
            _popup.PopupEntity(Loc.GetString(
                "cp-lock-lock-remove-fail-locked",
                ("lock", MetaData(args.EntityUid).EntityName),
                ("target", MetaData(lockSlot).EntityName)), lockSlot);
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

        LockLock(lockComp);
        _appearance.SetData(lockSlot, LockSlotVisuals.LockExist, true);
    }

    private void OnLockRemoved(Entity<CPLockSlotComponent> lockSlot, ref EntRemovedFromContainerMessage args)
    {
        if (args.Container.ID != lockSlot.Comp.LockSlotId)
            return;
        _appearance.SetData(lockSlot, LockSlotVisuals.LockExist, false);
    }

    public abstract bool TryUseKeyOnLock(EntityUid user, Entity<CPKeyComponent> keyEnt, Entity<CPLockComponent> lockEnt);
    public abstract void LockLock(CPLockComponent lockEnt);
    public abstract void UnlockLock(CPLockComponent lockEnt);
}

[Serializable, NetSerializable]
public enum LockSlotVisuals : byte
{
    LockExist
}
