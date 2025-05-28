using Content.Shared._CP14.Trading.Prototypes;
using Content.Shared.Stacks;
using Robust.Client.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._CP14.Trading.BuyServices;

public sealed partial class CP14BuyItemsService : CP14StoreBuyService
{
    [DataField(required: true)]
    public EntProtoId Product;

    [DataField]
    public int Count = 1;

    public override void Buy(EntityManager entManager,
        IPrototypeManager prototype,
        EntityUid platform)
    {
        for (var i = 0; i < Count; i++)
        {
            entManager.SpawnNextToOrDrop(Product, platform);
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

    public override string GetDesc(IPrototypeManager protoMan)
    {
        if (!protoMan.TryIndex(Product, out var indexedProduct))
            return string.Empty;

        return indexedProduct.Description;
    }
}
