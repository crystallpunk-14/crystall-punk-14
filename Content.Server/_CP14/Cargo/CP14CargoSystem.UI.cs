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

    private void OnBeforeUIOpen(Entity<CP14TradingInfoBoardComponent> ent, ref BeforeActivatableUIOpenEvent args)
    {
        UpdateUIProducts(ent);
    }

    private void UpdateUIProducts(Entity<CP14TradingInfoBoardComponent> ent)
    {
        if (!TryComp<CP14StationTravelingStoreShipComponent>(Transform(ent).MapUid, out var stationTrade))
            return;

        var prodBuy = new HashSet<CP14StoreUiProductEntry>();

        //Add special buy positions
        foreach (var (proto, price) in stationTrade.CurrentSpecialBuyPositions)
        {
            var name = proto.NameOverride is null ? proto.Service.GetName(_proto) : Loc.GetString(proto.NameOverride);
            var desc = new StringBuilder();
            desc.Append("\n" + Loc.GetString("cp14-store-buy-hint", ("name", name), ("code", "[color=yellow][bold]#" + proto.Code + "[/bold][/color]")));

            prodBuy.Add(new CP14StoreUiProductEntry(proto.ID, proto.IconOverride ?? proto.Service.GetTexture(_proto), proto.Service.GetEntityView(_proto), name, desc.ToString(), price, true));
        }

        //Add static buy positions
        foreach (var (proto, price) in stationTrade.CurrentBuyPositions)
        {
            var name = proto.NameOverride is null ? proto.Service.GetName(_proto) : Loc.GetString(proto.NameOverride);
            var desc = new StringBuilder();
            desc.Append("\n" + Loc.GetString("cp14-store-buy-hint", ("name", name), ("code", "[color=yellow][bold]#" + proto.Code + "[/bold][/color]")));

            prodBuy.Add(new CP14StoreUiProductEntry(proto.ID,  proto.IconOverride ?? proto.Service.GetTexture(_proto), proto.Service.GetEntityView(_proto), name, desc.ToString(), price, false));
        }

        _userInterface.SetUiState(ent.Owner, CP14StoreUiKey.Key, new CP14StoreUiState(prodBuy));
    }
}
