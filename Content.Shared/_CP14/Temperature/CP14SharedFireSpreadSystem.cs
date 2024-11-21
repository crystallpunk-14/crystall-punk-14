using Content.Shared.DoAfter;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;

namespace Content.Shared._CP14.Temperature;

public abstract partial class CP14SharedFireSpreadSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14FireSpreadComponent, OnFireChangedEvent>(OnFireChangedSpread);
        SubscribeLocalEvent<CP14DespawnOnExtinguishComponent, OnFireChangedEvent>(OnFireChangedDespawn);
        SubscribeLocalEvent<CP14DelayedIgnitionSourceComponent, AfterInteractEvent>(OnDelayedIgniteAttempt);
    }

    private void OnFireChangedDespawn(Entity<CP14DespawnOnExtinguishComponent> ent, ref OnFireChangedEvent args)
    {
        if (!args.OnFire)
            QueueDel(ent);
    }

    private void OnFireChangedSpread(Entity<CP14FireSpreadComponent> ent, ref OnFireChangedEvent args)
    {
        if (args.OnFire)
        {
            EnsureComp<CP14ActiveFireSpreadingComponent>(ent);
        }
        else
        {
            if (HasComp<CP14ActiveFireSpreadingComponent>(ent))
                RemCompDeferred<CP14ActiveFireSpreadingComponent>(ent);
        }

        ent.Comp.NextSpreadTime = _gameTiming.CurTime + TimeSpan.FromSeconds(ent.Comp.SpreadCooldownMax);
    }

    private void OnDelayedIgniteAttempt(Entity<CP14DelayedIgnitionSourceComponent> ent, ref AfterInteractEvent args)
    {
        if (!args.CanReach || args.Handled || args.Target == null)
            return;

        var time = ent.Comp.Delay;
        var caution = true;

        if (TryComp<CP14IgnitionModifierComponent>(args.Target, out var modifier))
        {
            time *= modifier.IgnitionTimeModifier;
            caution = !modifier.HideCaution;
        }

        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager,
            args.User,
            time,
            new CP14IgnitionDoAfter(),
            args.Target,
            args.Target,
            args.Used)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            BreakOnDropItem = true,
            BreakOnHandChange = true,
            BlockDuplicate = true,
        });

        var selfMessage = Loc.GetString("cp14-attempt-ignite-caution-self",
            ("target", MetaData(args.Target.Value).EntityName));
        var otherMessage = Loc.GetString("cp14-attempt-ignite-caution",
            ("name", Identity.Entity(args.User, EntityManager)),
            ("target", Identity.Entity(args.Target.Value, EntityManager)));
        _popup.PopupPredicted(selfMessage,
            otherMessage,
            args.User,
            args.User,
            caution ? PopupType.MediumCaution : PopupType.Small);
    }
}

/// <summary>
/// Raised whenever an FlammableComponent OnFire is Changed
/// </summary>
[ByRefEvent]
public readonly record struct OnFireChangedEvent(bool OnFire)
{
    public readonly bool OnFire = OnFire;
}

[Serializable, NetSerializable]
public sealed partial class CP14IgnitionDoAfter : SimpleDoAfterEvent
{
}
