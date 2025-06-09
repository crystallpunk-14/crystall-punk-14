
using Content.Shared._CP14.Religion.Components;
using Content.Shared._CP14.Religion.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Religion.Systems;

public abstract partial class CP14SharedReligionGodSystem
{
    private void InitializeFollowers()
    {

    }

    public bool TryToBelieve(EntityUid target, ProtoId<CP14ReligionPrototype> religion)
    {
        if (!_proto.TryIndex(religion, out var indexedReligion))
            return false;

        EnsureComp<CP14ReligionFollowerComponent>(target, out var follower);

        if (!follower.CanBecomeFollower)
            return false;

        follower.Religion = religion;
        follower.CanBecomeFollower = false;
        Dirty(target, follower);

        EditObservation(target, religion, indexedReligion.FollowerObservationRadius);
        return true;
    }

    public void ToDisbelieve(EntityUid target, ProtoId<CP14ReligionPrototype> religion)
    {
        if (!_proto.TryIndex(religion, out var indexedReligion))
            return;

        EnsureComp<CP14ReligionFollowerComponent>(target, out var follower);

        if (follower.Religion != religion)
            return;

        follower.Religion = null;
        Dirty(target, follower);

        EditObservation(target, religion, -indexedReligion.FollowerObservationRadius);
    }
}
