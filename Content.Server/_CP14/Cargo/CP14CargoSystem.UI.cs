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

        if (!TryComp<CP14StationTravelingStoreShipTargetComponent>(ent.Comp.Station.Value, out var storeTargetComp))
            return;

        var prodBuy = new HashSet<CP14StoreUiProductEntry>();
        var prodSell = new HashSet<CP14StoreUiProductEntry>();

        //Add special buy positions
        foreach (var (proto, price) in storeTargetComp.CurrentSpecialBuyPositions)
        {
            var name = Loc.GetString(proto.Name);
            var desc = new StringBuilder();
            desc.Append(Loc.GetString(proto.Desc) + "\n");
            desc.Append("\n" + Loc.GetString("cp14-store-buy-hint", ("name", Loc.GetString(proto.Name)), ("code", "[color=yellow][bold]#" + proto.Code + "[/bold][/color]")));

            prodBuy.Add(new CP14StoreUiProductEntry(proto.ID, proto.Icon, name, desc.ToString(), price, true));
        }

        //Add static buy positions
        foreach (var (proto, price) in storeTargetComp.CurrentBuyPositions)
        {
            var name = Loc.GetString(proto.Name);
            var desc = new StringBuilder();
            desc.Append(Loc.GetString(proto.Desc) + "\n");
            desc.Append("\n" + Loc.GetString("cp14-store-buy-hint", ("name", Loc.GetString(proto.Name)), ("code", "[color=yellow][bold]#" + proto.Code + "[/bold][/color]")));

            prodBuy.Add(new CP14StoreUiProductEntry(proto.ID, proto.Icon, name, desc.ToString(), price, false));
        }

        //Add special sell positions
        foreach (var (proto, price) in storeTargetComp.CurrentSpecialSellPositions)
        {
            var name = Loc.GetString(proto.Name);

            var desc = new StringBuilder();
            desc.Append(Loc.GetString(proto.Desc) + "\n");
            desc.Append("\n" + Loc.GetString("cp14-store-sell-hint", ("name", Loc.GetString(proto.Name))));

            prodSell.Add(new CP14StoreUiProductEntry(proto.ID, proto.Icon, name, desc.ToString(), price, true));
        }

        //Add static sell positions
        foreach (var proto in storeTargetComp.CurrentSellPositions)
        {
            var name = Loc.GetString(proto.Key.Name);

            var desc = new StringBuilder();
            desc.Append(Loc.GetString(proto.Key.Desc) + "\n");
            desc.Append("\n" + Loc.GetString("cp14-store-sell-hint", ("name", Loc.GetString(proto.Key.Name))));

            prodSell.Add(new CP14StoreUiProductEntry(proto.Key.ID, proto.Key.Icon, name, desc.ToString(), proto.Value, false));
        }

        var stationComp = storeTargetComp;
        _userInterface.SetUiState(ent.Owner, CP14StoreUiKey.Key, new CP14StoreUiState(prodBuy, prodSell, stationComp.OnStation, stationComp.NextTravelTime));
    }
}
