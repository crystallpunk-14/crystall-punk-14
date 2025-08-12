using System.Numerics;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Shared._CP14.MagicRitual.Effects;

public sealed partial class SpawnEntity : CP14RitualEffect
{
    [DataField(required: true)]
    public EntProtoId Proto = default!;

    [DataField]
    public int Count = 1;

    [DataField]
    public float Offset = 1f;

    public override void Effect(IEntityManager entMan, IPrototypeManager protoMan, EntityUid ritual)
    {
        var random = IoCManager.Resolve<IRobustRandom>();
        var origin = entMan.GetComponent<TransformComponent>(ritual).Coordinates;

        for (var i = 0; i < Count; i++)
        {
            var spawnPosition = origin.Offset(new Vector2(random.NextFloat(-Offset, Offset), random.NextFloat(-Offset, Offset)));
            var ent = entMan.SpawnAtPosition(Proto, spawnPosition);
        }
    }
}
