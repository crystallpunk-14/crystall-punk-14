using Content.Shared._CP14.Skill;
using Content.Shared._CP14.Skill.Components;
using Content.Shared._CP14.Transmutation.Components;
using Content.Shared._CP14.Transmutation.Prototypes;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Transmutation;

public sealed class CP14TransmutationSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly CP14SharedSkillSystem _skill = default!;

    public bool TryTransmutate(Entity<CP14TransmutableComponent?> ent, ProtoId<CP14TransmutationPrototype> method, EntityUid transmutator)
    {
        if (!Resolve(ent, ref ent.Comp))
            return false;

        if (ent.Comp.Resource is null)
            return false;

        if (!ent.Comp.Entries.TryGetValue(method, out var targetEntProto))
            return false;

        if (!_proto.TryIndex(method, out var indexedMethod))
            return false;

        if (!TryComp<CP14SkillStorageComponent>(transmutator, out var skillStorage))
            return false;

        var skillpoints = skillStorage.SkillPoints;
        if (!skillpoints.TryGetValue(ent.Comp.Resource.Value, out var currentPoints))
            return false;

        if (!_proto.TryIndex(ent.Comp.Resource.Value, out var indexedResource))
            return false;

        var freePoints = currentPoints.Max - currentPoints.Sum;

        if (freePoints < ent.Comp.Cost)
        {
            _popup.PopupEntity(Loc.GetString("cp14-magic-skillpointcost", ("name", Loc.GetString(indexedResource.Name)), ("count", ent.Comp.Cost - freePoints)), ent, transmutator);
            return false;
        }

        _skill.RemoveSkillPoints(transmutator, ent.Comp.Resource.Value, ent.Comp.Cost, false, skillStorage);

        var position = Transform(ent).Coordinates;

        //VFX
        if (_net.IsClient && indexedMethod.VFX is not null)
            SpawnAtPosition(indexedMethod.VFX, position);

        //Audio
        if (indexedMethod.Sound is not null)
            _audio.PlayPredicted(indexedMethod.Sound, position, transmutator);

        SpawnAtPosition(targetEntProto, position);
        QueueDel(ent);

        return true;
    }
}
