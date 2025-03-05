using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._CP14.Cargo.Prototype.BuyServices;

public sealed partial class CP14UnlockPositionsService : CP14StoreBuyService
{
    [DataField]
    public HashSet<ProtoId<CP14StoreBuyPositionPrototype>> AddBuyPositions = new();

    [DataField]
    public HashSet<ProtoId<CP14StoreSellPositionPrototype>> AddSellPositions = new();

    [DataField]
    public HashSet<ProtoId<CP14StoreBuyPositionPrototype>> RemoveBuyPositions = new();

    [DataField]
    public HashSet<ProtoId<CP14StoreSellPositionPrototype>> RemoveSellPositions = new();

    [DataField]
    public SpriteSpecifier? Icon = null;

    [DataField]
    public LocId Name = string.Empty;

    public override void Buy(EntityManager entManager, IPrototypeManager prototype, Entity<CP14TradingPortalComponent> portal)
    {
        foreach (var buy in AddBuyPositions)
        {
            if (!prototype.TryIndex(buy, out var indexedBuy))
                continue;

            portal.Comp.AvailableBuyPosition.Add(indexedBuy);
        }

        foreach (var sell in AddSellPositions)
        {
            if (!prototype.TryIndex(sell, out var indexedSell))
                continue;

            portal.Comp.AvailableSellPosition.Add(indexedSell);
        }

        foreach (var rBuy in RemoveBuyPositions)
        {
            if (!prototype.TryIndex(rBuy, out var indexedBuy))
                continue;

            if (!portal.Comp.AvailableBuyPosition.Contains(indexedBuy))
                continue;

            portal.Comp.AvailableBuyPosition.Remove(indexedBuy);
        }

        foreach (var rSell in RemoveSellPositions)
        {
            if (!prototype.TryIndex(rSell, out var indexedSell))
                continue;

            if (!portal.Comp.AvailableSellPosition.Contains(indexedSell))
                continue;

            portal.Comp.AvailableSellPosition.Remove(indexedSell);
        }
    }

    public override string GetName(IPrototypeManager protoMan)
    {
        return Loc.GetString(Name);
    }

    public override EntProtoId? GetEntityView(IPrototypeManager protoManager)
    {
        return null;
    }

    public override SpriteSpecifier? GetTexture(IPrototypeManager protoManager)
    {
        return Icon;
    }
}
