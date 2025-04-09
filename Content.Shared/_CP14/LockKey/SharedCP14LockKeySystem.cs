using Content.Shared._CP14.LockKey.Components;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Lock;
using Content.Shared.Popups;
using Content.Shared.Storage;
using Content.Shared.Verbs;
using Content.Shared.Doors.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Random;
using Robust.Shared.Serialization;

namespace Content.Shared._CP14.LockKey;

public sealed class SharedCP14LockKeySystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly LockSystem _lock = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    private EntityQuery<LockComponent> _lockQuery;
    private EntityQuery<CP14LockComponent> _cp14LockQuery;
    private EntityQuery<CP14KeyComponent> _keyQuery;
    private EntityQuery<DoorComponent> _doorQuery;

    public const int DepthComplexity = 2;

    public override void Initialize()
    {
        base.Initialize();

        _lockQuery = GetEntityQuery<LockComponent>();
        _cp14LockQuery = GetEntityQuery<CP14LockComponent>();
        _keyQuery = GetEntityQuery<CP14KeyComponent>();
        _doorQuery = GetEntityQuery<DoorComponent>();

        SubscribeLocalEvent<CP14KeyComponent, AfterInteractEvent>(OnKeyInteract);
        SubscribeLocalEvent<CP14KeyRingComponent, AfterInteractEvent>(OnKeyRingInteract);
        SubscribeLocalEvent<CP14KeyComponent, GetVerbsEvent<UtilityVerb>>(OnKeyToLockVerb);

        SubscribeLocalEvent<CP14LockpickComponent, GetVerbsEvent<UtilityVerb>>(OnLockPickToLockVerb);
        SubscribeLocalEvent<CP14LockComponent, LockPickHackDoAfterEvent>(OnLockHacked);
    }

    private void OnKeyRingInteract(Entity<CP14KeyRingComponent> keyring, ref AfterInteractEvent args)
    {
        if (args.Handled)
            return;

        if (!args.CanReach || args.Target is not { Valid: true })
            return;

        if (!TryComp<StorageComponent>(keyring, out var storageComp))
            return;

        if (!_lockQuery.TryComp(args.Target, out _))
            return;

        if (!_cp14LockQuery.TryComp(args.Target, out var cp14LockComp))
            return;

        foreach (var (key, _) in storageComp.StoredItems)
        {
            if (!_keyQuery.TryComp(key, out var keyComp))
                continue;

            if (keyComp.LockShape != cp14LockComp.LockShape)
                continue;

            TryUseKeyOnLock(args.User, new Entity<CP14LockComponent>(args.Target.Value, cp14LockComp), new Entity<CP14KeyComponent>(key, keyComp));
            args.Handled = true;
            return;
        }

        _popup.PopupEntity(Loc.GetString("cp14-lock-key-no-fit"), args.Target.Value, args.User);
    }

    private void OnKeyInteract(Entity<CP14KeyComponent> key, ref AfterInteractEvent args)
    {
        if (args.Handled)
            return;

        if (!args.CanReach || args.Target is not { Valid: true })
            return;

        if (!_lockQuery.TryComp(args.Target, out _))
            return;

        if (!_cp14LockQuery.TryComp(args.Target, out var cp14LockComp))
            return;

        TryUseKeyOnLock(args.User, new Entity<CP14LockComponent>(args.Target.Value, cp14LockComp), key);
        args.Handled = true;
    }

    private void OnLockPickToLockVerb(Entity<CP14LockpickComponent> lockPick, ref GetVerbsEvent<UtilityVerb> args)
    {
        if (!args.CanInteract || !args.CanAccess)
            return;

        if (!_lockQuery.TryComp(args.Target, out var lockComp) || !lockComp.Locked)
            return;

        if (!_cp14LockQuery.TryComp(args.Target, out _))
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
                    _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, user, lockPick.Comp.HackTime, new LockPickHackDoAfterEvent(height), target, target, lockPick)
                    {
                        BreakOnDamage = true,
                        BreakOnMove = true,
                        BreakOnDropItem = true,
                        BreakOnHandChange = true,
                    });
                },
                Text = Loc.GetString("cp14-lock-verb-lock-pick-use-text") + $" {height}",
                Message = Loc.GetString("cp14-lock-verb-lock-pick-use-message"),
                Category = VerbCategory.CP14LockPick,
                Priority = height,
                CloseMenu = false,
            };

            args.Verbs.Add(verb);
        }
    }

    private void OnLockHacked(Entity<CP14LockComponent> ent, ref LockPickHackDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        if (ent.Comp.LockShape == null)
            return;

        if (!_lockQuery.TryComp(ent, out var lockComp))
            return;

        if (!TryComp<CP14LockpickComponent>(args.Used, out var lockPick))
            return;

        if (args.Height == ent.Comp.LockShape[ent.Comp.LockPickStatus]) //Success
        {
            _audio.PlayPvs(lockPick.SuccessSound, ent);
            ent.Comp.LockPickStatus++;
            if (ent.Comp.LockPickStatus >= ent.Comp.LockShape.Count) // Final success
            {
                if (lockComp.Locked)
                {
                    _lock.TryUnlock(ent, args.User, lockComp);
                    _popup.PopupEntity(Loc.GetString("cp14-lock-unlock", ("lock", MetaData(ent).EntityName)), ent, args.User);
                    ent.Comp.LockPickStatus = 0;
                    return;
                }

                _lock.TryLock(ent, args.User, lockComp);
                _popup.PopupEntity(Loc.GetString("cp14-lock-lock", ("lock", MetaData(ent).EntityName)), ent, args.User);
                ent.Comp.LockPickStatus = 0;
                return;
            }
            _popup.PopupEntity(Loc.GetString("cp14-lock-lock-pick-success"), ent, args.User);
        }
        else //Fail
        {
            _audio.PlayPvs(lockPick.FailSound, ent);
            if (_random.Prob(ent.Comp.LockPickDamageChance)) // Damage lock pick
            {
                lockPick.Health--;
                if (lockPick.Health > 0)
                {
                    _popup.PopupEntity(Loc.GetString("cp14-lock-lock-pick-failed-damage", ("lock", MetaData(ent).EntityName)), ent, args.User);
                }
                else
                {
                    _popup.PopupEntity(Loc.GetString("cp14-lock-lock-pick-failed-break", ("lock", MetaData(ent).EntityName)), ent, args.User);
                    QueueDel(args.Used);
                }
            }
            else
            {
                _popup.PopupEntity(Loc.GetString("cp14-lock-lock-pick-failed", ("lock", MetaData(ent).EntityName)), ent, args.User);
            }
            ent.Comp.LockPickedFailMarkup = true;
            ent.Comp.LockPickStatus = 0;
        }
    }

    private void OnKeyToLockVerb(Entity<CP14KeyComponent> key, ref GetVerbsEvent<UtilityVerb> args)
    {
        if (!args.CanInteract || !args.CanAccess)
            return;

        if (!_lockQuery.TryComp(args.Target, out var lockComp))
            return;

        if (!_cp14LockQuery.TryComp(args.Target, out var cp14LockComponent))
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

        if (_doorQuery.TryComp(target, out var doorComponent) && doorComponent.State == DoorState.Open)
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
                    _popup.PopupEntity(Loc.GetString("cp14-lock-unlock", ("lock", MetaData(target).EntityName)), target, user);
            }
            else
            {
                if (_lock.TryLock(target, user))
                    _popup.PopupEntity(Loc.GetString("cp14-lock-lock", ("lock", MetaData(target).EntityName)), target, user);
            }
        }
        else
        {
            _popup.PopupEntity(Loc.GetString("cp14-lock-key-no-fit"), target, user);
        }
    }
}

[Serializable, NetSerializable]
public sealed partial class LockPickHackDoAfterEvent : DoAfterEvent
{
    [DataField]
    public int Height = 0;

    public LockPickHackDoAfterEvent(int h)
    {
        Height = h;
    }

    public override DoAfterEvent Clone() => this;
}
