using Content.Shared._CP14.MagicRitual.Prototypes;
using Content.Shared.Follower;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.MagicRitual;

public partial class CP14SharedRitualSystem : EntitySystem
{
    [Dependency] private readonly FollowerSystem _followerSystem = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly INetManager _net = default!;

    public void ChangeRitualStability(EntityUid uid, float dStab, CP14MagicRitualComponent? ritual = null)
    {
        //if (!Resolve(uid, ref ritual))
        //    return;
//
        //var newS = MathHelper.Clamp01(ritual.Stability + dStab);
//
        //var ev = new CP14RitualStabilityChangedEvent(ritual.Stability, newS);
        //RaiseLocalEvent(uid, ev);
//
        //ritual.Stability = newS;
    }

    public void AddOrbToRitual(EntityUid uid, EntProtoId orb, CP14MagicRitualComponent? ritual = null)
    {
        //if (_net.IsClient)
        //    return;
//
        //if (!Resolve(uid, ref ritual))
        //    return;
//
        //if (!_proto.TryIndex(orb, out var indexedOrb))
        //    return;
//
        //if (ritual.Orbs.Count >= ritual.MaxOrbCapacity)
        //    return;
//
        //var spawnedOrb = Spawn(orb, _transform.GetMapCoordinates(uid));
//
        //if (!TryComp<CP14MagicRitualOrbComponent>(spawnedOrb, out var orbComp))
        //{
        //    QueueDel(spawnedOrb);
        //    return;
        //}
//
        //_followerSystem.StartFollowingEntity(spawnedOrb, uid);
        //ritual.Orbs.Add((spawnedOrb, orbComp));
    }

    public void ConsumeOrbType(EntityUid uid, ProtoId<CP14MagicTypePrototype> magicType, CP14MagicRitualComponent? ritual = null)
    {
        //if (!Resolve(uid, ref ritual))
        //    return;
//
        //foreach (var orb in ritual.Orbs)
        //{
        //    var powers = orb.Comp.Powers;
        //    if (!powers.ContainsKey(magicType))
        //        continue;
//
        //    ritual.Orbs.Remove(orb);
        //    QueueDel(orb);
        //    return;
        //}
    }
}
