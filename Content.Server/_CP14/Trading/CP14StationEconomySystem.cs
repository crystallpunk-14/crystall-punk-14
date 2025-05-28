using Content.Server.Cargo.Systems;
using Content.Server.Station.Events;
using Content.Shared._CP14.Trading.BuyServices;
using Content.Shared._CP14.Trading.Components;
using Content.Shared._CP14.Trading.Prototypes;
using Content.Shared._CP14.Trading.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._CP14.Trading;

public sealed partial class CP14StationEconomySystem : CP14SharedStationEconomySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly PricingSystem _price = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    public override void Initialize()
    {
        base.Initialize();
        InitPriceEvents();

        SubscribeLocalEvent<CP14StationEconomyComponent, StationPostInitEvent>(OnStationPostInit);
    }

    private void OnStationPostInit(Entity<CP14StationEconomyComponent> ent, ref StationPostInitEvent args)
    {
        UpdatePricing(ent);
    }

    private void UpdatePricing(Entity<CP14StationEconomyComponent> ent)
    {
        ent.Comp.Pricing.Clear();
        foreach (var trade in _proto.EnumeratePrototypes<CP14TradingPositionPrototype>())
        {
            double price = 0;
            switch (trade.Service)
            {
                case CP14BuyItemsService buyItems:
                    if (!_proto.TryIndex(buyItems.Product, out var indexedProduct))
                        break;
                    price += _price.GetEstimatedPrice(indexedProduct) * buyItems.Count;
                    break;
            }

            price += trade.PriceMarkup;

            //Random fluctuation
            price *= 1 + _random.NextFloat(trade.PriceFluctuation);

            ent.Comp.Pricing.TryAdd(trade, (int) price);
        }
        Dirty(ent);
    }
}
