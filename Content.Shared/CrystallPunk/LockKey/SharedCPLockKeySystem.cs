
using Content.Shared.Containers.ItemSlots;
using Content.Shared.CrystallPunk.LockKey;
using Content.Shared.Popups;
using Robust.Shared.Containers;

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
        //SubscribeLocalEvent<CPLockSlotComponent, EntRemovedFromContainerMessage>(OnLockRemoveAttempt);
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

        if (lockComp.Locked && lockComp.RoundstartLockInit)
        {
            _popup.PopupEntity(Loc.GetString(
                "cp-lock-lock-insert-fail-locked",
                ("lock", MetaData(args.EntityUid).EntityName),
                ("target", MetaData(lockSlot).EntityName)), lockSlot);
            args.Cancel();
        }
        lockComp.RoundstartLockInit = true;
    }

    protected bool TryUseKeyOnLock(EntityUid user, Entity<CPKeyComponent> keyEnt, Entity<CPLockComponent> lockEnt)
    {
        var keyShape = keyEnt.Comp.LockShape;
        var lockShape = lockEnt.Comp.LockShape;

        if (keyShape == null || lockShape == null)
            return false;

        var keyFits = keyShape == lockShape;
        if (keyFits)
        {
            if (lockEnt.Comp.Locked)
            {
                _popup.PopupEntity(Loc.GetString("cp-lock-unlock-lock", ("lock", MetaData(lockEnt.Owner).EntityName)), lockEnt.Owner, user);
                UnlockLock(lockEnt.Comp);
            }
            else
            {
                _popup.PopupEntity(Loc.GetString("cp-lock-lock-lock", ("lock", MetaData(lockEnt.Owner).EntityName)), lockEnt.Owner, user);
                LockLock(lockEnt.Comp);
            }
            return true;
        }
        else
        {
            _popup.PopupEntity(Loc.GetString("cp-lock-key-use-nofit"), lockEnt, user);
        }
        return false;
    }
    private void LockLock(CPLockComponent lockEnt)
    {
        lockEnt.Locked = true;
    }

    private void UnlockLock(CPLockComponent lockEnt)
    {
        lockEnt.Locked = false;
    }
}
