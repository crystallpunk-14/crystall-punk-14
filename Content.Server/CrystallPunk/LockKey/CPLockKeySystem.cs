
using Content.Server.GameTicking.Events;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.CrystallPunk.LockKey;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Lock;
using Content.Shared.Verbs;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Content.Server.CrystallPunk.LockKey;


public sealed partial class CPLockKeySystem : SharedCPLockKeySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;
    [Dependency] private readonly LockSystem _lock = default!;

    private Dictionary<ProtoId<CPLockCategoryPrototype>, List<int>> _roundKeyData = new();

    private const int DepthCompexity = 2;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RoundStartingEvent>(OnRoundStart);

        SubscribeLocalEvent<CPLockComponent, MapInitEvent>(OnLockInit);
        SubscribeLocalEvent<CPKeyComponent, MapInitEvent>(OnKeyInit);

        SubscribeLocalEvent<CPKeyComponent, GetVerbsEvent<UtilityVerb>>(OnKeyToLockVerb);

        SubscribeLocalEvent<CPKeyComponent, AfterInteractEvent>(OnKeyInteract);

        SubscribeLocalEvent<CPKeyComponent, ExaminedEvent>(OnKeyExamine);
        SubscribeLocalEvent<CPLockComponent, ExaminedEvent>(OnLockExamine);
        SubscribeLocalEvent<CPLockSlotComponent, ExaminedEvent>(OnLockSlotExamine);

    }

    private void OnKeyInteract(Entity<CPKeyComponent> key, ref AfterInteractEvent args)
    {
        if (args.Handled)
            return;

        if (!args.CanReach || args.Target is not { Valid: true } target)
            return;

        if (TryComp<CPLockComponent>(args.Target, out var lockComp))
        {
            if (TryUseKeyOnLock(args.User, key, new Entity<CPLockComponent>(target, lockComp)))
                args.Handled = true;
            return;
        }
        if (TryComp<CPLockSlotComponent>(args.Target, out var lockSlot) && TryGetLockFromSlot(args.Target.Value, out var lockEnt))
        {
            if (TryUseKeyOnLock(args.User, key, lockEnt.Value))
                args.Handled = true;
            return;
        }
    }

    private void OnKeyToLockVerb(Entity<CPKeyComponent> key, ref GetVerbsEvent<UtilityVerb> args)
    {
        if (!args.CanInteract || !args.CanAccess)
            return;

        var target = args.Target;
        CPLockComponent? lockComp = null;

        TryComp(args.Target, out lockComp);
        if (lockComp == null && TryComp<CPLockSlotComponent>(args.Target, out var lockSlot) && TryGetLockFromSlot(args.Target, out var lockEnt))
        {
            lockComp = lockEnt.Value.Comp;
            target = lockEnt.Value.Owner;
        }

        if (lockComp == null)
            return;

        var user = args.User;

        var verb = new UtilityVerb()
        {
            Act = () =>
            {
                TryUseKeyOnLock(user, key, new Entity<CPLockComponent>(target, lockComp));
            },
            IconEntity = GetNetEntity(key),
            Text = Loc.GetString(lockComp.Locked ? "cp-lock-verb-use-key-text-open" : "cp-lock-verb-use-key-text-close", ("item", MetaData(args.Target).EntityName)),
            Message = Loc.GetString("cp-lock-verb-use-key-message", ("item", MetaData(args.Target).EntityName))
        };

        args.Verbs.Add(verb);
    }

    #region Init
    private void OnRoundStart(RoundStartingEvent ev)
    {
        _roundKeyData = new();
    }

    private void OnKeyInit(Entity<CPKeyComponent> keyEnt, ref MapInitEvent args)
    {
        if (keyEnt.Comp.AutoGenerateKey != null)
        {
            keyEnt.Comp.LockShape = GetKeyLockData(keyEnt.Comp.AutoGenerateKey.Value);
        }
    }

    private void OnLockInit(Entity<CPLockComponent> lockEnt, ref MapInitEvent args)
    {
        if (lockEnt.Comp.AutoGenerateLock != null)
        {
            lockEnt.Comp.LockShape = GetKeyLockData(lockEnt.Comp.AutoGenerateLock.Value);
        }
    }
    #endregion

    private void OnKeyExamine(Entity<CPKeyComponent> key, ref ExaminedEvent args)
    {
        if (key.Comp.LockShape == null)
            return;

        var markup = Loc.GetString("cp-lock-examine-key", ("item", MetaData(key).EntityName));
        markup += " (";
        foreach (var item in key.Comp.LockShape)
        {
            markup += $"{item} ";
        }
        markup += ")";
        args.PushMarkup(markup);
    }

    private void OnLockExamine(Entity<CPLockComponent> lockEnt, ref ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString(lockEnt.Comp.Locked ? "cp-lock-examine-lock-closed" : "cp-lock-examine-lock-open", ("lock", MetaData(lockEnt).EntityName)));
    }

    private void OnLockSlotExamine(Entity<CPLockSlotComponent> lockSlot, ref ExaminedEvent args)
    {
        if (!TryGetLockFromSlot(lockSlot, out var lockEnt)) return;

        var markup = Loc.GetString(lockEnt.Value.Comp.Locked ? "cp-lock-examine-lock-slot-locked" : "cp-lock-examine-lock-slot-unlocked", ("lock", MetaData(lockEnt.Value).EntityName));

        args.PushMarkup(markup);
    }


    private List<int> GetKeyLockData(ProtoId<CPLockCategoryPrototype> category)
    {
        if (_roundKeyData.ContainsKey(category))
            return _roundKeyData[category];
        else
        {
            var newData = GenerateNewUniqueLockData(category);
            _roundKeyData[category] = newData;
            return newData;
        }
    }

    public override bool TryGetLockFromSlot(EntityUid uid,
    [NotNullWhen(true)] out Entity<CPLockComponent>? lockEnt,
    CPLockSlotComponent? component = null)
    {
        if (!Resolve(uid, ref component, false))
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

    private List<int> GenerateNewUniqueLockData(ProtoId<CPLockCategoryPrototype> category)
    {
        List<int> newKeyData = new List<int>();
        var categoryData = _proto.Index(category);
        var ready = false;
        var iteration = 0;

        while (!ready)
        {
            //Generate try
            newKeyData = new List<int>();
            for (int i = 0; i < categoryData.Complexity; i++)
            {
                newKeyData.Add(_random.Next(-DepthCompexity, DepthCompexity));
            }

            //Identity Check shitcode
            // На текущий момент он пытается сгенерировать уникальный код. Если он 100 раз не смог сгенерировать уникальный код, он выдаст последний сгенерированный неуникальный.
            var unique = true;
            foreach (var pair in _roundKeyData)
            {
                if (newKeyData.SequenceEqual(pair.Value))
                {
                    unique = false;
                    break;
                }
            }
            if (unique)
                return newKeyData;
            else
                iteration++;

            if (iteration > 100)
            {
                break;
            }
        }
        Log.Error("The unique key for CPLockSystem could not be generated!");
        return newKeyData; //FUCK
    }

    public override bool TryUseKeyOnLock(EntityUid user, Entity<CPKeyComponent> keyEnt, Entity<CPLockComponent> lockEnt)
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
                _popup.PopupEntity(Loc.GetString("cp-lock-unlock-lock", ("lock", MetaData(lockEnt).EntityName)), lockEnt, user);
                UnlockLock(lockEnt);
            }
            else
            {
                _popup.PopupEntity(Loc.GetString("cp-lock-lock-lock", ("lock", MetaData(lockEnt).EntityName)), lockEnt, user);
                LockLock(lockEnt);
            }
            return true;
        }
        else
        {
            _popup.PopupEntity(Loc.GetString("cp-lock-key-use-nofit"), lockEnt, user);
        }
        return false;
    }

    public override void LockLock(Entity<CPLockComponent> lockEnt)
    {
        lockEnt.Comp.Locked = true;

    }

    public override void UnlockLock(Entity<CPLockComponent> lockEnt)
    {
        lockEnt.Comp.Locked = false;
    }
}
