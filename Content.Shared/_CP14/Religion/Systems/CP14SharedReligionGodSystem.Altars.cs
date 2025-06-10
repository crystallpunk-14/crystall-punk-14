
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

    }

    public bool TryConvertAltar(EntityUid target, ProtoId<CP14ReligionPrototype> religion)
    {
        if (!_proto.TryIndex(religion, out var indexedReligion))
            return false;

        EnsureComp<CP14ReligionAltarComponent>(target, out var altar);

        if (!altar.CanBeConverted)
            return false;

        altar.Religion = religion;
        Dirty(target, altar);

        EditObservation(target, religion, indexedReligion.AltarObservationRadius);
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
        altar.Religion = null;
        Dirty(target, altar);
    }
}
