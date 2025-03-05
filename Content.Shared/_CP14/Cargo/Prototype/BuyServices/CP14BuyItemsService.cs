using Content.Shared.Stacks;
using Content.Shared.Storage.EntitySystems;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._CP14.Cargo.Prototype.BuyServices;

public sealed partial class CP14BuyItemsService : CP14StoreBuyService
{
    [DataField(required: true)]
    public EntProtoId Product;

    [DataField]
    public int Count = 1;

    public override void Buy(EntityManager entManager,
        IPrototypeManager prototype,
        Entity<CP14TradingPortalComponent> portal)
    {
        var storageSystem = entManager.System<SharedEntityStorageSystem>();

        for (var i = 0; i < Count; i++)
        {
            var spawned = entManager.Spawn(Product);
            storageSystem.Insert(spawned, portal);
        }
    }

    public override string GetName(IPrototypeManager protoMan)
    {
        if (!protoMan.TryIndex(Product, out var indexedProduct))
            return ":3";

        var count = Count;
        if (indexedProduct.TryGetComponent<StackComponent>(out var stack))
            count *= stack.Count;

        return Count > 0 ? $"{indexedProduct.Name} x{count}" : indexedProduct.Name;
    }

    public override EntProtoId? GetEntityView(IPrototypeManager protoManager)
    {
        return Product;
    }

    public override SpriteSpecifier? GetTexture(IPrototypeManager protoManager)
    {
        return null;
    }
}
