using System.Text;
using Content.Shared._CP14.Cargo;
using Content.Shared.UserInterface;

namespace Content.Server._CP14.Cargo;

public sealed partial class CP14CargoSystem
{
    public void InitializeUI()
    {
        SubscribeLocalEvent<CP14TradingInfoBoardComponent, BeforeActivatableUIOpenEvent>(OnBeforeUIOpen);
    }

    private void TryInitStore(Entity<CP14TradingInfoBoardComponent> ent)
    {
        //TODO: more accurate way to find the trading portal, without lookup
        var entitiesInRange = _lookup.GetEntitiesInRange<CP14TradingPortalComponent>(Transform(ent).Coordinates, 3);
        foreach (var trading in entitiesInRange)
        {
            ent.Comp.TradingPortal = trading;
            break;
        }
    }

    private void OnBeforeUIOpen(Entity<CP14TradingInfoBoardComponent> ent, ref BeforeActivatableUIOpenEvent args)
    {
        //TODO: If you open a store on a mapping, and initStore() it, the entity will throw an error when you try to save the grid\map.

        if (ent.Comp.TradingPortal is null)
            TryInitStore(ent);

        UpdateUIProducts(ent);
    }

    private void UpdateUIProducts(Entity<CP14TradingInfoBoardComponent> ent)
    {
        if (ent.Comp.TradingPortal is null)
            return;

        if (!TryComp<CP14TradingPortalComponent>(ent.Comp.TradingPortal.Value, out var tradePortalComp))
            return;

        var prodBuy = new HashSet<CP14StoreUiProductEntry>();
        var prodSell = new HashSet<CP14StoreUiProductEntry>();

        //Add special buy positions
        foreach (var (proto, price) in tradePortalComp.CurrentSpecialBuyPositions)
        {
            var name = Loc.GetString(proto.Name);
            var desc = new StringBuilder();
            desc.Append(Loc.GetString(proto.Desc) + "\n");
            desc.Append("\n" + Loc.GetString("cp14-store-buy-hint", ("name", Loc.GetString(proto.Name)), ("code", "[color=yellow][bold]#" + proto.Code + "[/bold][/color]")));

            prodBuy.Add(new CP14StoreUiProductEntry(proto.ID, proto.Icon, name, desc.ToString(), price, true));
        }

        //Add static buy positions
        foreach (var (proto, price) in tradePortalComp.CurrentBuyPositions)
        {
            var name = Loc.GetString(proto.Name);
            var desc = new StringBuilder();
            desc.Append(Loc.GetString(proto.Desc) + "\n");
            desc.Append("\n" + Loc.GetString("cp14-store-buy-hint", ("name", Loc.GetString(proto.Name)), ("code", "[color=yellow][bold]#" + proto.Code + "[/bold][/color]")));

            prodBuy.Add(new CP14StoreUiProductEntry(proto.ID, proto.Icon, name, desc.ToString(), price, false));
        }

        //Add special sell positions
        foreach (var (proto, price) in tradePortalComp.CurrentSpecialSellPositions)
        {
            var name = proto.Service.GetName(_proto);

            var desc = new StringBuilder();
            desc.Append(Loc.GetString(proto.Desc) + "\n");
            desc.Append("\n" + Loc.GetString("cp14-store-sell-hint", ("name", name)));

            prodSell.Add(new CP14StoreUiProductEntry(proto.ID, proto.Icon, name, desc.ToString(), price, true));
        }

        //Add static sell positions
        foreach (var proto in tradePortalComp.CurrentSellPositions)
        {
            var name = proto.Key.Service.GetName(_proto);

            var desc = new StringBuilder();
            desc.Append(Loc.GetString(proto.Key.Desc) + "\n");
            desc.Append("\n" + Loc.GetString("cp14-store-sell-hint", ("name", name)));

            prodSell.Add(new CP14StoreUiProductEntry(proto.Key.ID, proto.Key.Icon, name, desc.ToString(), proto.Value, false));
        }

        var stationComp = tradePortalComp;
        _userInterface.SetUiState(ent.Owner, CP14StoreUiKey.Key, new CP14StoreUiState(prodBuy, prodSell));
    }
}
