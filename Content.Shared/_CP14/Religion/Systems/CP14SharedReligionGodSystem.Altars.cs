
using Content.Shared._CP14.Religion.Components;
using Content.Shared._CP14.Religion.Prototypes;
using Content.Shared.Verbs;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Religion.Systems;

public abstract partial class CP14SharedReligionGodSystem
{
    private void InitializeAltars()
    {
        SubscribeLocalEvent<CP14ReligionAltarComponent, GetVerbsEvent<ActivationVerb>>(GetBaseVerb);
        SubscribeLocalEvent<CP14ReligionAltarComponent, GetVerbsEvent<AlternativeVerb>>(GetAltVerb);
    }

    private void GetBaseVerb(Entity<CP14ReligionAltarComponent> ent, ref GetVerbsEvent<ActivationVerb> args)
    {

    }

    private void GetAltVerb(Entity<CP14ReligionAltarComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (ent.Comp.Religion is null)
            return;

        bool disabled = !CanBecomeFollower(args.User, ent.Comp.Religion.Value);

        if (!disabled && TryComp<CP14ReligionPendingFollowerComponent>(args.User, out var pendingFollower))
        {
            if (pendingFollower.Religion is not null)
                disabled = true;
        }

        var user = args.User;
        args.Verbs.Add(new AlternativeVerb()
        {
            Text = Loc.GetString("cp14-altar-become-follower"),
            Message = Loc.GetString("cp14-altar-become-follower-desc"),
            ConfirmationPopup = true,
            Disabled = disabled,
            Act = () =>
            {
                TryAddPendingFollower(user, ent.Comp.Religion.Value);
            },
        });
    }

    public bool TryConvertAltar(EntityUid target, ProtoId<CP14ReligionPrototype> religion)
    {
        if (!_proto.TryIndex(religion, out var indexedReligion))
            return false;

        EnsureComp<CP14ReligionAltarComponent>(target, out var altar);

        if (!altar.CanBeConverted)
            return false;

        var oldReligion = altar.Religion;
        altar.Religion = religion;
        Dirty(target, altar);

        EditObservation(target, religion, indexedReligion.AltarObservationRadius);

        var ev = new CP14ReligionChangedEvent(oldReligion, religion);
        RaiseLocalEvent(target, ev);

        return true;
    }

    public void DeconvertAltar(EntityUid target)
    {
        if (!TryComp<CP14ReligionAltarComponent>(target, out var altar))
            return;

        if (altar.Religion is null)
            return;

        if (!_proto.TryIndex(altar.Religion, out var indexedReligion))
            return;

        EditObservation(target, altar.Religion.Value, -indexedReligion.AltarObservationRadius);

        var oldReligion = altar.Religion;
        altar.Religion = null;

        var ev = new CP14ReligionChangedEvent(oldReligion, null);
        RaiseLocalEvent(target, ev);

        Dirty(target, altar);
    }
}
