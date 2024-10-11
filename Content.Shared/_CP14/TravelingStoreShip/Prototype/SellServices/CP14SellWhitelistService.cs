using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.TravelingStoreShip.Prototype.SellServices;

public sealed partial class CP14SellWhitelistService : CP14StoreSellService
{
    [DataField(required: true)]
    public EntityWhitelist Whitelist = new();

    [DataField(required: true)]
    public int Count = 1;

    public override bool TrySell(EntityManager entManager, HashSet<EntityUid> entities)
    {
        var whitelistSystem = entManager.System<EntityWhitelistSystem>();

        HashSet<EntityUid> suitable = new();

        int needCount = Count;
        foreach (var ent in entities)
        {
            if (needCount <= 0)
                break;

            if (!entManager.TryGetComponent<MetaDataComponent>(ent, out var metaData) || metaData.EntityPrototype is null)
                continue;

            if (!whitelistSystem.IsValid(Whitelist, ent))
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

    public override string? GetDescription(IPrototypeManager prototype, IEntityManager entSys)
    {
        return null;
    }
}
