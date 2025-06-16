using Content.Shared._CP14.Religion.Components;
using Content.Shared._CP14.Religion.Prototypes;
using Content.Shared.DoAfter;
using Content.Shared.Verbs;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._CP14.Religion.Systems;

public abstract partial class CP14SharedReligionGodSystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;

    private void InitializeAltars()
    {
        SubscribeLocalEvent<CP14ReligionAltarComponent, GetVerbsEvent<AlternativeVerb>>(GetAltVerb);
    }

    private void GetAltVerb(Entity<CP14ReligionAltarComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (ent.Comp.Religion is null)
            return;

        var disabled = !CanBecomeFollower(args.User, ent.Comp.Religion.Value);

        if (!disabled && TryComp<CP14ReligionPendingFollowerComponent>(args.User, out var pendingFollower))
        {
            if (pendingFollower.Religion is not null)
                disabled = true;
        }

        if (disabled)
            return;

        var user = args.User;
        args.Verbs.Add(new AlternativeVerb()
        {
            Text = Loc.GetString("cp14-altar-become-follower"),
            Message = Loc.GetString("cp14-altar-become-follower-desc"),
            Act = () =>
            {
                var doAfterArgs = new DoAfterArgs(EntityManager, user, 5f, new CP14AltarOfferDoAfter(), ent, used: ent)
                {
                    BreakOnDamage = true,
                    BreakOnMove = true,
                };
                _doAfter.TryStartDoAfter(doAfterArgs);
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

[Serializable, NetSerializable]
public sealed partial class CP14AltarOfferDoAfter : SimpleDoAfterEvent
{
}
