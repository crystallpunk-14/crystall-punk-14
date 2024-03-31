
using Content.Shared.Containers.ItemSlots;
using Content.Shared.CrystallPunk.LockKey;
using Content.Shared.Doors;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Lock;
using Content.Shared.Medical;
using Content.Shared.Popups;
using Robust.Shared.Containers;
using Robust.Shared.Serialization;
using System.Diagnostics.CodeAnalysis;
using Content.Shared.Verbs;

namespace Content.Shared.CrystallPunk.LockKey;

/// <summary>
/// 
/// </summary>
public sealed class SharedCPLockKeySystem : EntitySystem
{

    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly LockSystem _lock = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LockComponent, ContainerIsInsertingAttemptEvent>(OnLockInsertAttempt);

        SubscribeLocalEvent<LockComponent, EntInsertedIntoContainerMessage>(OnLockInserted);
        SubscribeLocalEvent<LockComponent, EntRemovedFromContainerMessage>(OnLockRemoved);

        SubscribeLocalEvent<CPKeyComponent, AfterInteractEvent>(OnKeyInteract);
        SubscribeLocalEvent<CPKeyComponent, GetVerbsEvent<UtilityVerb>>(OnKeyToLockVerb);
    }
    private void OnKeyInteract(Entity<CPKeyComponent> key, ref AfterInteractEvent args)
    {
        if (args.Handled)
            return;

        if (!args.CanReach || args.Target is not { Valid: true } target)
            return;

        if (TryComp<LockComponent>(args.Target, out var lockComp) &&
            TryGetLockFromSlot(args.Target.Value, out var lockEnt))
        {
            TryUseKeyOnLock(args.User, args.Target.Value, key, new Entity<CPLockComponent>(lockEnt.Value.Owner, lockEnt.Value.Comp));
            args.Handled = true;
        }
    }

    private void OnKeyToLockVerb(Entity<CPKeyComponent> key, ref GetVerbsEvent<UtilityVerb> args)
    {
        if (!args.CanInteract || !args.CanAccess)
            return;

        if (!TryComp<LockComponent>(args.Target, out var lockComp))
            return;

        if (!TryGetLockFromSlot(args.Target, out var lockItem))
            return;

        if (!TryComp<CPLockComponent>(lockItem, out var lockItemComp))
            return;

        var target = args.Target;
        var user = args.User;

        var verb = new UtilityVerb()
        {
            Act = () =>
            {
                TryUseKeyOnLock(user, target, key, new Entity<CPLockComponent>(target, lockItemComp));
            },
            IconEntity = GetNetEntity(key),
            Text = Loc.GetString(lockComp.Locked ? "cp-lock-verb-use-key-text-open" : "cp-lock-verb-use-key-text-close", ("item", MetaData(args.Target).EntityName)),
            Message = Loc.GetString("cp-lock-verb-use-key-message", ("item", MetaData(args.Target).EntityName))
        };

        args.Verbs.Add(verb);
    }

    private void OnLockInsertAttempt(Entity<LockComponent> lockSlot, ref ContainerIsInsertingAttemptEvent args)
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

        //if (lockComp.Locked)
        //{
        //    _popup.PopupEntity(Loc.GetString(
        //        "cp-lock-lock-insert-fail-locked",
        //        ("lock", MetaData(args.EntityUid).EntityName),
        //        ("target", MetaData(lockSlot).EntityName)), lockSlot);
        //    args.Cancel();
        //}
    }

    private void OnLockInserted(Entity<LockComponent> lockSlot, ref EntInsertedIntoContainerMessage args)
    {
        if (!lockSlot.Comp.Initialized)
            return;

        if (args.Container.ID != lockSlot.Comp.LockSlotId)
            return;

        if (!TryComp<CPLockComponent>(args.Entity, out var lockComp))
            return;

        _appearance.SetData(lockSlot, LockSlotVisuals.LockExist, true);
    }

    private void OnLockRemoved(Entity<LockComponent> lockSlot, ref EntRemovedFromContainerMessage args)
    {
        if (args.Container.ID != lockSlot.Comp.LockSlotId)
            return;
        _appearance.SetData(lockSlot, LockSlotVisuals.LockExist, false);
    }

    public bool TryGetLockFromSlot(EntityUid uid,
    [NotNullWhen(true)] out Entity<CPLockComponent>? lockEnt,
    LockComponent? component = null)
    {
        if (!Resolve(uid, ref component, false))
        {
            lockEnt = null;
            return false;
        }

        if (component.LockSlotId == null)
        {
            lockEnt = null;
            return false;
        }

        if (_itemSlots.TryGetSlot(uid, component.LockSlotId, out ItemSlot? slot))
        {
            if (TryComp<CPLockComponent>(slot.Item, out var lockComp))
            {
                lockEnt = new Entity<CPLockComponent>(slot.Item.Value, lockComp);
                return true;
            }
            else
            {
                lockEnt = null;
                return false;
            }
        }

        lockEnt = null;
        return false;
    }
    private bool TryUseKeyOnLock(EntityUid user, EntityUid target, Entity<CPKeyComponent> keyEnt, Entity<CPLockComponent> lockEnt)
    {
        if (!TryComp<LockComponent>(target, out var lockComp))
            return false;

        var keyShape = keyEnt.Comp.LockShape;
        var lockShape = lockEnt.Comp.LockShape;

        if (keyShape == null || lockShape == null)
            return false;

        if (keyShape == lockShape)
        {
            if (lockComp.Locked)
            {
                if(_lock.TryUnlock(target, user))
                    _popup.PopupEntity(Loc.GetString("cp-lock-unlock-lock", ("lock", MetaData(lockEnt).EntityName)), lockEnt, user);
            }
            else
            {
                if (_lock.TryLock(target, user))
                    _popup.PopupEntity(Loc.GetString("cp-lock-lock-lock", ("lock", MetaData(lockEnt).EntityName)), lockEnt, user);
            }
            return true;
        }
        else
        {
            _popup.PopupEntity(Loc.GetString("cp-lock-key-use-nofit"), lockEnt, user);
        }
        return false;
    }
}

[Serializable, NetSerializable]
public enum LockSlotVisuals : byte
{
    LockExist
}
