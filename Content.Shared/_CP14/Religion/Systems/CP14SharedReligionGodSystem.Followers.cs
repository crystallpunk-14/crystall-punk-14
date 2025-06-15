using Content.Shared._CP14.MagicSpell.Spells;
using Content.Shared._CP14.Religion.Components;
using Content.Shared._CP14.Religion.Prototypes;
using Content.Shared.Actions;
using Content.Shared.Alert;
using Content.Shared.Follower;
using Content.Shared.Mind;
using Content.Shared.Mobs;
using Content.Shared.Verbs;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Religion.Systems;

public abstract partial class CP14SharedReligionGodSystem
{
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] protected readonly SharedMindSystem Mind = default!;
    [Dependency] private readonly FollowerSystem _follower = default!;

    [ValidatePrototypeId<AlertPrototype>]
    public const string AlertProto = "CP14DivineOffer";

    private void InitializeFollowers()
    {
        SubscribeLocalEvent<CP14ReligionPendingFollowerComponent, MapInitEvent>(OnPendingFollowerInit);
        SubscribeLocalEvent<CP14ReligionPendingFollowerComponent, ComponentShutdown>(OnPendingFollowerShutdown);
        SubscribeLocalEvent<CP14ReligionPendingFollowerComponent, CP14BreakDivineOfferEvent>(OnBreakDivineOffer);
        SubscribeLocalEvent<CP14ReligionPendingFollowerComponent, CP14GodTouchEvent>(OnGodTouch);

        SubscribeLocalEvent<CP14ReligionAltarComponent, CP14AltarOfferDoAfter>(OnOfferDoAfter);

        SubscribeLocalEvent<CP14ReligionFollowerComponent, CP14RenounceFromGodEvent>(OnRenounceFromGod);
        SubscribeLocalEvent<CP14ReligionFollowerComponent, GetVerbsEvent<AlternativeVerb>>(OnGetAltVerbs);
        SubscribeLocalEvent<CP14ReligionFollowerComponent, MobStateChangedEvent>(OnFollowerStateChange);
    }

    private void OnFollowerStateChange(Entity<CP14ReligionFollowerComponent> ent, ref MobStateChangedEvent args)
    {
        if (ent.Comp.Religion is null)
            return;

        switch (args.NewMobState)
        {
            case MobState.Critical:
                SendMessageToGods(ent.Comp.Religion.Value, Loc.GetString("cp14-critical-follower-message", ("name", MetaData(ent).EntityName)), ent);
                break;

            case MobState.Dead:
                SendMessageToGods(ent.Comp.Religion.Value, Loc.GetString("cp14-dead-follower-message", ("name", MetaData(ent).EntityName)), ent);
                break;
        }
    }

    private void OnGetAltVerbs(EntityUid uid, CP14ReligionFollowerComponent component, GetVerbsEvent<AlternativeVerb> args)
    {
        if (!TryComp<CP14ReligionEntityComponent>(args.User, out var god))
            return;

        if (god.Religion != component.Religion)
            return;

        args.Verbs.Add(new AlternativeVerb
        {
            Text = Loc.GetString("admin-player-actions-follow"),
            Act = () =>
            {
                _follower.StartFollowingEntity(args.User, uid);
            },
        });
    }

    private void OnRenounceFromGod(Entity<CP14ReligionFollowerComponent> ent, ref CP14RenounceFromGodEvent args)
    {
        ToDisbelieve(ent);
    }

    private void OnOfferDoAfter(Entity<CP14ReligionAltarComponent> ent, ref CP14AltarOfferDoAfter args)
    {
        if (args.Handled || args.Cancelled)
            return;

        if (ent.Comp.Religion is null)
            return;

        TryAddPendingFollower(args.User, ent.Comp.Religion.Value);

        args.Handled = true;
    }

    private void OnGodTouch(Entity<CP14ReligionPendingFollowerComponent> ent, ref CP14GodTouchEvent args)
    {
        if (args.Religion != ent.Comp.Religion)
            return;

        TryToBelieve(ent);
    }

    private void OnBreakDivineOffer(Entity<CP14ReligionPendingFollowerComponent> ent, ref CP14BreakDivineOfferEvent args)
    {
        RemCompDeferred<CP14ReligionPendingFollowerComponent>(ent);

        if (ent.Comp.Religion is null)
            return;

        SendMessageToGods(ent.Comp.Religion.Value, Loc.GetString("cp14-unoffer-soul-god-message", ("name", MetaData(ent).EntityName)), ent);
    }

    private void OnPendingFollowerInit(Entity<CP14ReligionPendingFollowerComponent> ent, ref MapInitEvent args)
    {
        _alerts.ShowAlert(ent, AlertProto);
    }

    private void OnPendingFollowerShutdown(Entity<CP14ReligionPendingFollowerComponent> ent, ref ComponentShutdown args)
    {
        _alerts.ClearAlert(ent, AlertProto);
    }

    private bool CanBecomeFollower(EntityUid target, ProtoId<CP14ReligionPrototype> religion)
    {
        if (HasComp<CP14ReligionEntityComponent>(target))
            return false;

        EnsureComp<CP14ReligionFollowerComponent>(target, out var follower);

        if (follower.Religion is not null)
            return false;

        return !follower.RejectedReligions.Contains(religion);
    }

    private void TryAddPendingFollower(EntityUid target, ProtoId<CP14ReligionPrototype> religion)
    {
        if (!CanBecomeFollower(target, religion))
            return;

        EnsureComp<CP14ReligionPendingFollowerComponent>(target, out var pendingFollower);
        pendingFollower.Religion = religion;

        SendMessageToGods(religion, Loc.GetString("cp14-offer-soul-god-message", ("name", MetaData(target).EntityName)), target);
    }

    private bool TryToBelieve(Entity<CP14ReligionPendingFollowerComponent> pending)
    {
        if (pending.Comp.Religion is null)
            return false;

        if (!_proto.TryIndex(pending.Comp.Religion, out var indexedReligion))
            return false;

        if (!CanBecomeFollower(pending, pending.Comp.Religion.Value))
            return false;

        EnsureComp<CP14ReligionFollowerComponent>(pending, out var follower);

        var oldReligion = follower.Religion;
        follower.Religion = pending.Comp.Religion;
        Dirty(pending, follower);

        EditObservation(pending, pending.Comp.Religion.Value, indexedReligion.FollowerObservationRadius);

        var ev = new CP14ReligionChangedEvent(oldReligion, pending.Comp.Religion);
        RaiseLocalEvent(pending, ev);

        RemCompDeferred<CP14ReligionPendingFollowerComponent>(pending);
        SendMessageToGods(pending.Comp.Religion.Value, Loc.GetString("cp14-become-follower-message", ("name", MetaData(pending).EntityName)), pending);

        _actions.AddAction(pending, ref follower.RenounceAction, follower.RenounceActionProto);
        _actions.AddAction(pending, ref follower.AppealAction, follower.AppealToGofProto);
        return true;
    }

    public void ToDisbelieve(EntityUid target)
    {
        if (!TryComp<CP14ReligionFollowerComponent>(target, out var follower))
            return;

        if (follower.Religion is null)
            return;

        if (!_proto.TryIndex(follower.Religion, out var indexedReligion))
            return;

        SendMessageToGods(follower.Religion.Value, Loc.GetString("cp14-remove-follower-message", ("name", MetaData(target).EntityName)), target);
        EditObservation(target, follower.Religion.Value, -indexedReligion.FollowerObservationRadius);

        var oldReligion = follower.Religion;
        follower.Religion = null;
        if (oldReligion is not null)
            follower.RejectedReligions.Add(oldReligion.Value);

        var ev = new CP14ReligionChangedEvent(oldReligion, null);
        RaiseLocalEvent(target, ev);

        Dirty(target, follower);

        _actions.RemoveAction(target, follower.RenounceAction);
        _actions.RemoveAction(target, follower.AppealAction);
    }
}

public sealed partial class CP14BreakDivineOfferEvent : BaseAlertEvent;

public sealed partial class CP14RenounceFromGodEvent : InstantActionEvent;
