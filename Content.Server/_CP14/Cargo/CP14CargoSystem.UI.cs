using System.Text;
using Content.Shared._CP14.Cargo;
using Content.Shared.UserInterface;

namespace Content.Server._CP14.Cargo;

public sealed partial class CP14CargoSystem
{
    public void InitializeUI()
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
        //TODO: If you open a store on a mapping, and initStore() it, the entity will throw an error when you try to save the grid\map.

        if (ent.Comp.Station is null)
            TryInitStore(ent);

        UpdateUIProducts(ent);
    }

    private void UpdateAllStores()
    {
        //TODO: redo
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

            var name = Loc.GetString(proto.Key.Name);
            var desc = new StringBuilder();
            desc.Append(Loc.GetString(proto.Key.Desc) + "\n");
            foreach (var service in proto.Key.Services)
            {
                desc.Append(service.GetDescription(_proto, EntityManager));
            }

            desc.Append("\n" + Loc.GetString("cp14-store-buy-hint", ("name", Loc.GetString(proto.Key.Name)), ("code", "[color=yellow][bold]#" + proto.Key.Code + "[/bold][/color]")));

            prodBuy.Add(new CP14StoreUiProductEntry(proto.Key.ID, proto.Key.Icon, name, desc.ToString(), proto.Value));
        }

        foreach (var proto in ent.Comp.Station.Value.Comp.CurrentSellPositions)
        {
            var name = Loc.GetString(proto.Key.Name);

            var desc = new StringBuilder();
            desc.Append(Loc.GetString(proto.Key.Desc) + "\n");
            desc.Append(proto.Key.Service.GetDescription(_proto, EntityManager) + "\n");
            desc.Append("\n" + Loc.GetString("cp14-store-sell-hint", ("name", Loc.GetString(proto.Key.Name))));

            prodSell.Add(new CP14StoreUiProductEntry(proto.Key.ID, proto.Key.Icon, name, desc.ToString(), proto.Value));
        }

        var stationComp = ent.Comp.Station.Value.Comp;
        _userInterface.SetUiState(ent.Owner, CP14StoreUiKey.Key, new CP14StoreUiState(prodBuy, prodSell, stationComp.OnStation, stationComp.NextTravelTime));
    }
}
