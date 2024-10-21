using Content.Shared._CP14.LockKey.Components;
using Content.Shared.Interaction;
using Content.Shared.Lock;
using Content.Shared.Popups;
using Content.Shared.Storage;
using Content.Shared.Verbs;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Random;

namespace Content.Shared._CP14.LockKey;

public sealed class SharedCP14LockKeySystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly LockSystem _lock = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public const int DepthComplexity = 2;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14KeyComponent, AfterInteractEvent>(OnKeyInteract);
        SubscribeLocalEvent<CP14KeyRingComponent, AfterInteractEvent>(OnKeyRingInteract);
        SubscribeLocalEvent<CP14KeyComponent, GetVerbsEvent<UtilityVerb>>(OnKeyToLockVerb);
        SubscribeLocalEvent<CP14LockpickComponent, GetVerbsEvent<UtilityVerb>>(OnLockpickToLockVerb);
    }
    private void OnKeyRingInteract(Entity<CP14KeyRingComponent> keyring, ref AfterInteractEvent args)
    {
        if (args.Handled)
            return;

        if (!args.CanReach || args.Target is not { Valid: true })
            return;

        if (!TryComp<StorageComponent>(keyring, out var storageComp))
            return;

        if (!HasComp<LockComponent>(args.Target))
            return;

        if (!TryComp<CP14LockComponent>(args.Target, out var cp14LockComp))
            return;

        foreach (var (key, _) in storageComp.StoredItems)
        {
            if (!TryComp<CP14KeyComponent>(key, out var keyComp))
                continue;

            if (keyComp.LockShape != cp14LockComp.LockShape)
                continue;

            TryUseKeyOnLock(args.User, new Entity<CP14LockComponent>(args.Target.Value, cp14LockComp), new Entity<CP14KeyComponent>(key, keyComp));
            args.Handled = true;
            return;
        }

        _popup.PopupEntity(Loc.GetString("cp14-lock-keyring-use-nofit"), args.Target.Value, args.User);
    }

    private void OnKeyInteract(Entity<CP14KeyComponent> key, ref AfterInteractEvent args)
    {
        if (args.Handled)
            return;

        if (!args.CanReach || args.Target is not { Valid: true } target)
            return;

        if (!HasComp<LockComponent>(args.Target))
            return;

        if (!TryComp<CP14LockComponent>(args.Target, out var cp14LockComponent))
            return;

        TryUseKeyOnLock(args.User, new Entity<CP14LockComponent>(args.Target.Value, cp14LockComponent), key);
        args.Handled = true;
    }

    private void OnLockpickToLockVerb(Entity<CP14LockpickComponent> lockPick, ref GetVerbsEvent<UtilityVerb> args)
    {
        if (!args.CanInteract || !args.CanAccess)
            return;

        if (!TryComp<LockComponent>(args.Target, out var lockComp) || !lockComp.Locked)
            return;

        if (!TryComp<CP14LockComponent>(args.Target, out var cp14LockComponent))
            return;

        var target = args.Target;
        var user = args.User;

        for (var i = DepthComplexity; i >= -DepthComplexity; i--)
        {
            var height = i;
            var verb = new UtilityVerb()
            {
                Act = () =>
                {
                    TryHackDoorElement(user, (target, cp14LockComponent), lockPick, lockComp, height);
                },
                Text = Loc.GetString("cp14-lock-verb-lockpick-use-text") + $" {height}",
                Message = Loc.GetString("cp14-lock-verb-lockpick-use-message"),
                Category = VerbCategory.Lockpick,
                Priority = height,
                CloseMenu = false,
            };

            args.Verbs.Add(verb);
        }
    }

    private void TryHackDoorElement(EntityUid user,
        Entity<CP14LockComponent> target,
        Entity<CP14LockpickComponent> lockPick,
        LockComponent lockComp,
        int height)
    {
        if (target.Comp.LockShape == null)
            return;

        if (height == target.Comp.LockShape[target.Comp.LockPickStatus]) //Success
        {
            _audio.PlayPvs(lockPick.Comp.SuccessSound, target);
            target.Comp.LockPickStatus++;
            if (target.Comp.LockPickStatus >= target.Comp.LockShape.Count) // Final success
            {
                if (lockComp.Locked)
                {
                    _lock.TryUnlock(target, user, lockComp);
                    _popup.PopupEntity(Loc.GetString("cp14-lock-unlock-lock", ("lock", MetaData(target).EntityName)), target, user);
                    target.Comp.LockPickStatus = 0;
                    return;
                }

                _lock.TryLock(target, user, lockComp);
                _popup.PopupEntity(Loc.GetString("cp14-lock-lock-lock", ("lock", MetaData(target).EntityName)), target, user);
                target.Comp.LockPickStatus = 0;
                return;
            }
            _popup.PopupEntity(Loc.GetString("cp14-lock-lockpick-success"), target, user);
        }
        else //Fail
        {
            _audio.PlayPvs(lockPick.Comp.FailSound, target);
            if (_random.Prob(target.Comp.LockPickDamageChance)) // Damage lock pick
            {
                lockPick.Comp.Health--;
                if (lockPick.Comp.Health > 0)
                {
                    _popup.PopupEntity(Loc.GetString("cp14-lock-lockpick-failed-damage", ("lock", MetaData(target).EntityName)), target, user);
                }
                else
                {
                    _popup.PopupEntity(Loc.GetString("cp14-lock-lockpick-failed-break", ("lock", MetaData(target).EntityName)), target, user);
                    QueueDel(lockPick);
                }
            }
            else
            {
                _popup.PopupEntity(Loc.GetString("cp14-lock-lockpick-failed", ("lock", MetaData(target).EntityName)), target, user);
            }
            target.Comp.LockPickedFailMarkup = true;
            target.Comp.LockPickStatus = 0;
        }
    }

    private void OnKeyToLockVerb(Entity<CP14KeyComponent> key, ref GetVerbsEvent<UtilityVerb> args)
    {
        if (!args.CanInteract || !args.CanAccess)
            return;

        if (!TryComp<LockComponent>(args.Target, out var lockComp))
            return;

        if (!TryComp<CP14LockComponent>(args.Target, out var cp14LockComponent))
            return;

        var target = args.Target;
        var user = args.User;

        var verb = new UtilityVerb
        {
            Act = () =>
            {
                TryUseKeyOnLock(user, new Entity<CP14LockComponent>(target, cp14LockComponent), key);
            },
            IconEntity = GetNetEntity(key),
            Text = Loc.GetString(lockComp.Locked ? "cp14-lock-verb-use-key-text-open" : "cp14-lock-verb-use-key-text-close", ("item", MetaData(args.Target).EntityName)),
            Message = Loc.GetString("cp14-lock-verb-use-key-message", ("item", MetaData(args.Target).EntityName)),
        };

        args.Verbs.Add(verb);
    }

    private void TryUseKeyOnLock(EntityUid user, Entity<CP14LockComponent> target, Entity<CP14KeyComponent> key)
    {
        if (!TryComp<LockComponent>(target, out var lockComp))
            return;

        var keyShape = key.Comp.LockShape;
        var lockShape = target.Comp.LockShape;

        if (keyShape == null || lockShape == null)
            return;

        if (keyShape == lockShape)
        {
            if (lockComp.Locked)
            {
                if(_lock.TryUnlock(target, user))
                    _popup.PopupEntity(Loc.GetString("cp14-lock-unlock-lock", ("lock", MetaData(target).EntityName)), target, user);
            }
            else
            {
                if (_lock.TryLock(target, user))
                    _popup.PopupEntity(Loc.GetString("cp14-lock-lock-lock", ("lock", MetaData(target).EntityName)), target, user);
            }
        }
        else
        {
            _popup.PopupEntity(Loc.GetString("cp14-lock-key-use-nofit"), target, user);
        }
    }
}
