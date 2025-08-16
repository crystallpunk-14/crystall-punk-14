using Content.Shared._CP14.Transmutation.Components;
using Content.Shared._CP14.Transmutation.Prototypes;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Transmutation;

public sealed class CP14TransmutationSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public bool TryTransmutate(Entity<CP14TransmutableComponent?> ent, ProtoId<CP14TransmutationPrototype> method)
    {
        if (!Resolve(ent, ref ent.Comp))
            return false;

        if (!_proto.TryIndex(method, out var indexedMethod))
            return false;

        if (!ent.Comp.Entries.TryGetValue(method, out var targetEntProto))
            return false;

        var position = Transform(ent).Coordinates;

        //VFX
        if (_net.IsClient && indexedMethod.VFX is not null)
            SpawnAtPosition(indexedMethod.VFX, position);

        //Audio
        if (indexedMethod.Sound is not null)
            _audio.PlayPvs(indexedMethod.Sound, position);

        SpawnAtPosition(targetEntProto, position);
        QueueDel(ent);

        return true;
    }
}
