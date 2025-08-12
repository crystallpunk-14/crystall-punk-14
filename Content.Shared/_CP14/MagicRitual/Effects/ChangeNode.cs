using System.Numerics;
using Content.Shared._CP14.MagicRitual.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Shared._CP14.MagicRitual.Effects;

public sealed partial class ChangeNode : CP14RitualEffect
{
    [DataField(required: true)]
    public EntProtoId Proto = default!;

    public override void Effect(IEntityManager entMan, IPrototypeManager protoMan, EntityUid ritual)
    {
        if (!protoMan.TryIndex(Proto, out var indexedProto))
            return;

        var compFactory = IoCManager.Resolve<IComponentFactory>();
        if (!indexedProto.TryGetComponent<CP14RitualNodeComponent>(out var targetNode, compFactory))
        {
            Logger.Error($"Ritual node {indexedProto.ID} does not have a CP14RitualNodeComponent, cannot change node.");
            return;
        }

        var origin = entMan.GetComponent<TransformComponent>(ritual).Coordinates;
        entMan.SpawnAtPosition(Proto, origin);
        entMan.QueueDeleteEntity(ritual);
    }
}
