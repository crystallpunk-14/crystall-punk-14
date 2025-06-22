using Content.Shared._CP14.Trading.Components;
using Content.Shared._CP14.Trading.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Trading.Systems;

public abstract partial class CP14SharedStationEconomySystem : EntitySystem
{
    public int? GetPrice(ProtoId<CP14TradingPositionPrototype> position)
    {
        var query = EntityQueryEnumerator<CP14StationEconomyComponent>();

        while (query.MoveNext(out var uid, out var economy))
        {
            if (!economy.Pricing.TryGetValue(position, out var price))
                return null;

            return price;
        }

        return null;
    }

    public List<ProtoId<CP14TradingRequestPrototype>> GetRequests(ProtoId<CP14TradingFactionPrototype> faction)
    {
        var query = EntityQueryEnumerator<CP14StationEconomyComponent>();

        while (query.MoveNext(out var uid, out var economy))
        {
            if (!economy.ActiveRequests.TryGetValue(faction, out var requests))
                continue;

            return requests;
        }

        return [];
    }
}
