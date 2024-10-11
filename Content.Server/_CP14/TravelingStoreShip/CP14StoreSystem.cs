using System.Text;
using Content.Server.Station.Systems;
using Content.Shared._CP14.Currency;
using Content.Shared._CP14.TravelingStoreShip;
using Content.Shared.UserInterface;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._CP14.TravelingStoreShip;

public sealed class CP14StoreSystem : CP14SharedStoreSystem
{
    [Dependency] private readonly UserInterfaceSystem _userInterface = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly CP14CurrencySystem _cp14Currency = default!;
    [Dependency] private readonly StationSystem _station = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14StoreComponent, BeforeActivatableUIOpenEvent>(OnBeforeUIOpen);
    }

    private void TryInitStore(Entity<CP14StoreComponent> ent)
    {
        //TODO: There's no support for multiple stations. (settlements).
        var stations = _station.GetStations();

        if (stations.Count == 0)
            return;

        if (!TryComp<CP14StationTravelingStoreshipTargetComponent>(stations[0], out var station))
            return;

        ent.Comp.Station = new Entity<CP14StationTravelingStoreshipTargetComponent>(stations[0], station);
    }

    private void OnBeforeUIOpen(Entity<CP14StoreComponent> ent, ref BeforeActivatableUIOpenEvent args)
    {
        if (ent.Comp.Station is null)
            TryInitStore(ent);

        UpdateUIProducts(ent);
    }

    private void UpdateUIProducts(Entity<CP14StoreComponent> ent)
    {
        if (ent.Comp.Station is null)
            return;

        var prodBuy = new HashSet<CP14StoreUiProductEntry>();
        var prodSell = new HashSet<CP14StoreUiProductEntry>();

        foreach (var proto in ent.Comp.Station.Value.Comp.CurrentStorePositionsBuy)
        {
            if (!_proto.TryIndex(proto.Key, out var indexedProto))
                continue;

            var name = Loc.GetString(indexedProto.Title);
            var desc = new StringBuilder();
            foreach (var service in indexedProto.Services)
            {
                desc.Append(service.GetDescription(_proto, EntityManager));
            }

            prodBuy.Add(new CP14StoreUiProductEntry(proto.Key.Id, indexedProto.Icon, name, desc.ToString(), proto.Value));
        }

        foreach (var proto in ent.Comp.Station.Value.Comp.CurrentStorePositionsSell)
        {
            if (!_proto.TryIndex(proto.Key, out var indexedProto))
                continue;

            var name = Loc.GetString(indexedProto.Title);
            var desc = new StringBuilder();
            foreach (var service in indexedProto.Services)
            {
                desc.Append(service.GetDescription(_proto, EntityManager));
            }

            prodSell.Add(new CP14StoreUiProductEntry(proto.Key.Id, indexedProto.Icon, name, desc.ToString(), proto.Value));
        }

        var stationComp = ent.Comp.Station.Value.Comp;
        _userInterface.SetUiState(ent.Owner, CP14StoreUiKey.Key, new CP14StoreUiState(prodBuy, prodSell, stationComp.OnStation, stationComp.NextTravelTime, 150));
    }
}
