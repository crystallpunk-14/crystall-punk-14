
using Content.Shared._CP14.MagicSpell.Spells;
using Content.Shared._CP14.Religion.Components;
using Content.Shared._CP14.Religion.Prototypes;
using Content.Shared.Alert;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Religion.Systems;

public abstract partial class CP14SharedReligionGodSystem
{
    [Dependency] private readonly AlertsSystem _alerts = default!;
    private void InitializeFollowers()
    {
        SubscribeLocalEvent<CP14ReligionPendingFollowerComponent, MapInitEvent>(OnPendingFollowerInit);
        SubscribeLocalEvent<CP14ReligionPendingFollowerComponent, ComponentShutdown>(OnPendingFollowerShutdown);
        SubscribeLocalEvent<CP14ReligionPendingFollowerComponent, CP14BreakDivineOfferEvent>(OnBreakDivineOffer);
        SubscribeLocalEvent<CP14ReligionPendingFollowerComponent, CP14GodTouchEvent>(OnGodTouch);
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
        _alerts.ShowAlert(ent, "CP14DivineOffer");
    }

    private void OnPendingFollowerShutdown(Entity<CP14ReligionPendingFollowerComponent> ent, ref ComponentShutdown args)
    {
        _alerts.ClearAlert(ent, "CP14DivineOffer");
    }

    public bool CanBecomeFollower(EntityUid target, ProtoId<CP14ReligionPrototype> religion)
    {
        if (!TryComp<CP14ReligionFollowerComponent>(target, out var follower))
            return true;

        if (follower.CanBeConverted)
            return true;

        return false;
    }

    public void TryAddPendingFollower(EntityUid target, ProtoId<CP14ReligionPrototype> religion)
    {
        if (!_proto.TryIndex(religion, out var indexedReligion))
            return;

        if (!CanBecomeFollower(target, religion))
            return;

        EnsureComp<CP14ReligionPendingFollowerComponent>(target, out var pendingFollower);
        pendingFollower.Religion = religion;

        SendMessageToGods(religion, Loc.GetString("cp14-offer-soul-god-message", ("name", MetaData(target).EntityName)), target);
    }

    public bool TryToBelieve(Entity<CP14ReligionPendingFollowerComponent> pending)
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
        follower.CanBeConverted = false;
        Dirty(pending, follower);

        EditObservation(pending, pending.Comp.Religion.Value, indexedReligion.FollowerObservationRadius);

        var ev = new CP14ReligionChangedEvent(oldReligion, pending.Comp.Religion);
        RaiseLocalEvent(pending, ev);

        RemCompDeferred<CP14ReligionPendingFollowerComponent>(pending);
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

        EditObservation(target, follower.Religion.Value, -indexedReligion.FollowerObservationRadius);

        var oldReligion = follower.Religion;
        follower.Religion = null;

        var ev = new CP14ReligionChangedEvent(oldReligion, null);
        RaiseLocalEvent(target, ev);

        Dirty(target, follower);
    }
}

public sealed partial class CP14BreakDivineOfferEvent : BaseAlertEvent;
