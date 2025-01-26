using System.Text;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Cargo.Prototype.BuyServices;

public sealed partial class CP14UnlockPositionsService : CP14StoreBuyService
{
    [DataField]
    public HashSet<ProtoId<CP14StoreBuyPositionPrototype>> AddBuyPositions = new();

    [DataField]
    public HashSet<ProtoId<CP14StoreSellPositionPrototype>> AddSellPositions = new();

    [DataField]
    public HashSet<ProtoId<CP14StoreBuyPositionPrototype>> RemoveBuyPositions = new();

    public override void Buy(EntityManager entManager, IPrototypeManager prototype, Entity<CP14StationTravelingStoreShipTargetComponent> station)
    {
        foreach (var buy in AddBuyPositions)
        {
            if (!prototype.TryIndex(buy, out var indexedBuy))
                continue;

            if (station.Comp.AvailableBuyPosition.Contains(indexedBuy))
                continue;

            station.Comp.AvailableBuyPosition.Add(indexedBuy);
        }

        foreach (var sell in AddSellPositions)
        {
            if (!prototype.TryIndex(sell, out var indexedSell))
                continue;

            if (station.Comp.AvailableSellPosition.Contains(indexedSell))
                continue;

            station.Comp.AvailableSellPosition.Add(indexedSell);
        }

        foreach (var rBuy in RemoveBuyPositions)
        {
            if (!prototype.TryIndex(rBuy, out var indexedBuy))
                continue;

            if (!station.Comp.AvailableBuyPosition.Contains(indexedBuy))
                continue;

            station.Comp.AvailableBuyPosition.Remove(indexedBuy);
        }
    }
}
