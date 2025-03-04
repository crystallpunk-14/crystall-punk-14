using Content.Shared._CP14.Cargo;

namespace Content.Server._CP14.Cargo;

public sealed partial class CP14CargoSystem
{
    public void InitializePortals()
    {
        SubscribeLocalEvent<CP14TradingPortalComponent, MapInitEvent>(OnTradePortalMapInit);
        SubscribeLocalEvent<CP14TradingPortalComponent, EntityTerminatingEvent>(OnTradePortalRemove);
    }

    private void OnTradePortalMapInit(Entity<CP14TradingPortalComponent> ent, ref MapInitEvent args)
    {
        AddRoundstartTradingPositions(ent);
        UpdateStorePositions(ent);
    }

    private void OnTradePortalRemove(Entity<CP14TradingPortalComponent> ent, ref EntityTerminatingEvent args)
    {
        //TODO: return all items to the map
    }


    //private void OnPostInit(Entity<CP14StationTravelingStoreShipTargetComponent> station, ref StationPostInitEvent args)
    //{
    //    if (!Deleted(station.Comp.Shuttle))
    //        return;
//
    //    var member = EnsureComp<StationMemberComponent>(shuttle.Value);
    //    member.Station = station;
//
    //    var roundRemover = EnsureComp<CP14RoundRemoveShuttleComponent>(shuttle.Value);
    //    roundRemover.Station = station;
//
    //    station.Comp.NextTravelTime = _timing.CurTime + TimeSpan.FromSeconds(10f);
//
    //    AddRoundstartTradingPositions(station);
    //    UpdateStorePositions(station);
    //}
}
