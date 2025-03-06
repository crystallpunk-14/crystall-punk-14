using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._CP14.Cargo.Prototype.SellServices;

public sealed partial class CP14SellWhitelistService : CP14StoreSellService
{
    [DataField(required: true)]
    public EntityWhitelist Whitelist = new();

    [DataField(required: true)]
    public int Count = 1;

    [DataField(required: true)]
    public LocId Name = string.Empty;

    [DataField(required: true)]
    public SpriteSpecifier? Sprite = null;

    public override bool TrySell(EntityManager entManager, HashSet<EntityUid> entities)
    {
        var whitelistSystem = entManager.System<EntityWhitelistSystem>();

        HashSet<EntityUid> suitable = new();

        var needCount = Count;
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
            entities.Remove(selledEnt);
            entManager.QueueDeleteEntity(selledEnt);
        }

        return true;
    }

    public override string GetName(IPrototypeManager protoMan)
    {
        return $"{Loc.GetString(Name)} x{Count}";
    }

    public override EntProtoId? GetEntityView(IPrototypeManager protoManager)
    {
        return null;
    }

    public override SpriteSpecifier? GetTexture(IPrototypeManager protoManager)
    {
        return Sprite;
    }
}
