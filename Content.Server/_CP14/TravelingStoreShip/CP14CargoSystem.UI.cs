using System.Text;
using Content.Shared._CP14.TravelingStoreShip;
using Content.Shared.UserInterface;

namespace Content.Server._CP14.TravelingStoreShip;

public sealed partial class CP14CargoSystem
{
    public void InitializeStore()
    {
        SubscribeLocalEvent<CP14CargoStoreComponent, BeforeActivatableUIOpenEvent>(OnBeforeUIOpen);
    }

    private void TryInitStore(Entity<CP14CargoStoreComponent> ent)
    {
        //TODO: There's no support for multiple stations. (settlements).
        var stations = _station.GetStations();

        if (stations.Count == 0)
            return;

        if (!TryComp<CP14StationTravelingStoreShipTargetComponent>(stations[0], out var station))
            return;

        ent.Comp.Station = new Entity<CP14StationTravelingStoreShipTargetComponent>(stations[0], station);
    }

    private void OnBeforeUIOpen(Entity<CP14CargoStoreComponent> ent, ref BeforeActivatableUIOpenEvent args)
    {
        if (ent.Comp.Station is null)
            TryInitStore(ent);

        UpdateUIProducts(ent);
    }

    //TODO: redo
    private void UpdateAllStores()
    {
        var query = EntityQueryEnumerator<CP14CargoStoreComponent>();
        while (query.MoveNext(out var uid, out var store))
        {
            UpdateUIProducts((uid, store));
        }
    }

    private void UpdateUIProducts(Entity<CP14CargoStoreComponent> ent)
    {
        if (ent.Comp.Station is null)
            return;

        var prodBuy = new HashSet<CP14StoreUiProductEntry>();
        var prodSell = new HashSet<CP14StoreUiProductEntry>();

        foreach (var proto in ent.Comp.Station.Value.Comp.CurrentBuyPositions)
        {
            if (!_proto.TryIndex(proto.Key, out var indexedProto))
                continue;

            var name = Loc.GetString(indexedProto.Name);
            var desc = new StringBuilder();
            desc.Append(Loc.GetString(indexedProto.Desc) + "\n");
            foreach (var service in indexedProto.Services)
            {
                desc.Append(service.GetDescription(_proto, EntityManager));
            }

            prodBuy.Add(new CP14StoreUiProductEntry(proto.Key.Id, indexedProto.Icon, name, desc.ToString(), proto.Value));
        }

        foreach (var proto in ent.Comp.Station.Value.Comp.CurrentSellPositions)
        {
            if (!_proto.TryIndex(proto.Key, out var indexedProto))
                continue;

            var name = Loc.GetString(indexedProto.Name);

            var desc = new StringBuilder();
            desc.Append(Loc.GetString(indexedProto.Desc) + "\n");
            desc.Append(indexedProto.Service.GetDescription(_proto, EntityManager));

            prodSell.Add(new CP14StoreUiProductEntry(proto.Key.Id, indexedProto.Icon, name, desc.ToString(), proto.Value));
        }

        var stationComp = ent.Comp.Station.Value.Comp;
        _userInterface.SetUiState(ent.Owner, CP14StoreUiKey.Key, new CP14StoreUiState(prodBuy, prodSell, stationComp.OnStation, stationComp.NextTravelTime));
    }
}
