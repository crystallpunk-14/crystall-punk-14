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

    public void ChangeRitualStability(Entity<CP14MagicRitualComponent> ritual, float dStab)
    {
        var newS = MathHelper.Clamp01(ritual.Comp.Stability + dStab);

        var ev = new CP14RitualStabilityChangedEvent(ritual.Comp.Stability, newS);
        RaiseLocalEvent(ritual, ev);

        ritual.Comp.Stability = newS;
    }

    public void AddOrbToRitual(Entity<CP14MagicRitualComponent> ritual, EntProtoId orb)
    {
        if (_net.IsClient)
            return;

        if (!_proto.TryIndex(orb, out var indexedOrb))
            return;

        if (ritual.Comp.Orbs.Count >= ritual.Comp.MaxOrbCapacity)
            return;

        var spawnedOrb = Spawn(orb, _transform.GetMapCoordinates(ritual));

        if (!TryComp<CP14MagicRitualOrbComponent>(spawnedOrb, out var orbComp))
        {
            QueueDel(spawnedOrb);
            return;
        }

        _followerSystem.StartFollowingEntity(spawnedOrb, ritual);
        ritual.Comp.Orbs.Add((spawnedOrb, orbComp));
    }

    public void ConsumeOrbType(Entity<CP14MagicRitualComponent> ritual, ProtoId<CP14MagicTypePrototype> magicType)
    {
        foreach (var orb in ritual.Comp.Orbs)
        {
            var powers = orb.Comp.Powers;
            if (!powers.ContainsKey(magicType))
                continue;

            ritual.Comp.Orbs.Remove(orb);
            QueueDel(orb);
            return;
        }
    }
}
