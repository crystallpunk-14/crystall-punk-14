using System.Linq;
using Content.Server.Cargo.Systems;
using Content.Server.GameTicking;
using Content.Server.Station.Events;
using Content.Shared._CP14.Trading.BuyServices;
using Content.Shared._CP14.Trading.Components;
using Content.Shared._CP14.Trading.Prototypes;
using Content.Shared._CP14.Trading.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._CP14.Trading;

public sealed partial class CP14StationEconomySystem : CP14SharedStationEconomySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly PricingSystem _price = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14StationEconomyComponent, StationPostInitEvent>(OnStationPostInit);
        SubscribeLocalEvent<PrototypesReloadedEventArgs>(OnPrototypesReloaded);
    }

    private void OnPrototypesReloaded(PrototypesReloadedEventArgs ev)
    {
        if (!ev.WasModified<CP14TradingPositionPrototype>() && !ev.WasModified<CP14TradingRequestPrototype>())
            return;

        var query = EntityQueryEnumerator<CP14StationEconomyComponent>();
        while (query.MoveNext(out var uid, out var economyComponent))
        {
            UpdatePricing((uid, economyComponent));
            UpdateRequestPricing((uid, economyComponent));
        }
    }

    private void OnStationPostInit(Entity<CP14StationEconomyComponent> ent, ref StationPostInitEvent args)
    {
        UpdatePricing(ent);
        UpdateRequestPricing(ent);
        GenerateStartingRequests(ent);
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

    private void UpdateRequestPricing(Entity<CP14StationEconomyComponent> ent)
    {
        ent.Comp.RequestPricing.Clear();

        foreach (var trade in _proto.EnumeratePrototypes<CP14TradingRequestPrototype>())
        {
            double price = 0;
            foreach (var req in trade.Requirements)
            {
                price += req.GetPrice(EntityManager, _proto);
            }

            price += trade.AdditionalReward;

            ent.Comp.RequestPricing.TryAdd(trade, (int) price);
        }
        Dirty(ent);
    }

    public bool TryRerollRequest(ProtoId<CP14TradingFactionPrototype> faction,
        ProtoId<CP14TradingRequestPrototype> request)
    {
        var query = EntityQueryEnumerator<CP14StationEconomyComponent>();

        while (query.MoveNext(out var uid, out var economy))
        {
            if (!economy.ActiveRequests.TryGetValue(faction, out var requests))
                continue;

            if (!requests.Contains(request))
                continue;

            requests.Add(GetNextRequest(faction, requests) ?? request);
            requests.Remove(request);

            Dirty(uid, economy);
            return true;
        }

        return false;
    }

    private void GenerateStartingRequests(Entity<CP14StationEconomyComponent> ent)
    {
        ent.Comp.ActiveRequests.Clear();

        var allFactions = _proto.EnumeratePrototypes<CP14TradingFactionPrototype>();
        foreach (var faction in allFactions)
        {
            var requests = new HashSet<ProtoId<CP14TradingRequestPrototype>>();
            for (int i = 0; i < ent.Comp.MaxRequestCount; i++)
            {
                var nextRequest = GetNextRequest(faction.ID, requests);

                if (nextRequest == null)
                    break; // No more suitable requests

                requests.Add(nextRequest);
            }
            ent.Comp.ActiveRequests.Add(faction, requests);
        }
    }

    private CP14TradingRequestPrototype? GetNextRequest(ProtoId<CP14TradingFactionPrototype> faction, HashSet<ProtoId<CP14TradingRequestPrototype>> existing)
    {
        Dictionary<CP14TradingRequestPrototype, float> suitableRequestsWeights = new();

        var allRequests = _proto.EnumeratePrototypes<CP14TradingRequestPrototype>();
        foreach (var request in allRequests)
        {
            var passed = true;

            if (existing.Contains(request))
                passed = false;

            if (!request.PossibleFactions.Contains(faction))
                passed = false;

            var stationTime = _timing.CurTime.Subtract(_gameTicker.RoundStartTimeSpan);

            if (passed && TimeSpan.FromMinutes(request.FromMinutes) > stationTime)
                passed = false;

            if (passed && request.ToMinutes.HasValue && TimeSpan.FromMinutes(request.ToMinutes.Value) < stationTime)
                passed = false;

            if (passed)
                suitableRequestsWeights.Add(request, request.GenerationWeight);
        }

        return RequestPick(suitableRequestsWeights, _random);
    }

    /// <summary>
    /// Optimization moment: avoid re-indexing for weight selection
    /// </summary>
    private static CP14TradingRequestPrototype? RequestPick(Dictionary<CP14TradingRequestPrototype, float> weights, IRobustRandom random)
    {
        if (weights.Count == 0)
            return null; // No suitable requests

        var picks = weights;
        var sum = picks.Values.Sum();
        var accumulated = 0f;

        var rand = random.NextFloat() * sum;

        foreach (var (key, weight) in picks)
        {
            accumulated += weight;

            if (accumulated >= rand)
            {
                return key;
            }
        }

        // Shouldn't happen
        throw new InvalidOperationException($"Invalid weighted pick in CP14StationEconomySystem!");
    }
}
