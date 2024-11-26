using Content.Shared._CP14.ModularCraft;
using Content.Shared._CP14.ModularCraft.Components;
using Content.Shared.Throwing;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._CP14.ModularCraft;

public sealed class CP14ModularCraftSystem : CP14SharedModularCraftSystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public void DisassembleModular(EntityUid target)
    {
        if (!TryComp<CP14ModularCraftStartPointComponent>(target, out var modular))
            return;

        var sourceCoord = _transform.GetMapCoordinates(target);

        //Spawn source startpoint
        if (modular.InstalledParts.Count > 0)
        {
            var meta = MetaData(target).EntityPrototype;
            if (meta is not null)
            {
                var spawned = Spawn(meta.ID, sourceCoord);
                _throwing.TryThrow(spawned, _random.NextAngle().ToWorldVec(), 1f);
            }
        }

        //Spawn parts
        foreach (var part in modular.InstalledParts)
        {
            if (!_proto.TryIndex(part, out var indexedPart))
                continue;

            if (_random.Prob(indexedPart.DestroyProb))
                continue;

            var spawned = Spawn(indexedPart.SourcePart, sourceCoord);
            _throwing.TryThrow(spawned, _random.NextAngle().ToWorldVec(), 1f);
        }

        //Delete
        QueueDel(target);
    }
}
