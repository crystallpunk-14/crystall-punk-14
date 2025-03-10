using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._CP14.Cargo.Prototype.SellServices;

public sealed partial class CP14SellPrototypeService : CP14StoreSellService
{
    [DataField(required: true)]
    public EntProtoId Proto;

    [DataField]
    public int Count = 1;

    public override bool TrySell(EntityManager entManager, HashSet<EntityUid> entities)
    {
        HashSet<EntityUid> suitable = new();

        var needCount = Count;
        foreach (var ent in entities)
        {
            if (needCount <= 0)
                break;

            if (!entManager.TryGetComponent<MetaDataComponent>(ent, out var metaData))
                continue;

            if (metaData.EntityPrototype is null)
                continue;

            if (metaData.EntityPrototype != Proto)
                continue;

            suitable.Add(ent);
            needCount -= 1;
        }

        if (needCount > 0)
            return false;

        foreach (var selledEnt in suitable)
        {
            entManager.QueueDeleteEntity(selledEnt);
        }

        return true;
    }

    public override string GetName(IPrototypeManager protoMan)
    {
        if (!protoMan.TryIndex(Proto, out var proto))
            return ":3";

        return $"{proto.Name} x{Count}";
    }

    public override EntProtoId? GetEntityView(IPrototypeManager protoManager)
    {
        return Proto;
    }

    public override SpriteSpecifier? GetTexture(IPrototypeManager protoManager)
    {
        return null;
    }
}
