using Content.Server.Storage.Components;
using Content.Shared._CP14.Cargo;
using Content.Shared.Storage.Components;

namespace Content.Server._CP14.Cargo;

public sealed partial class CP14CargoSystem
{
    private void InitializePortals()
    {
        SubscribeLocalEvent<CP14TradingPortalComponent, MapInitEvent>(OnTradePortalMapInit);

        SubscribeLocalEvent<CP14TradingPortalComponent, StorageAfterCloseEvent>(OnTradePortalClose);
        SubscribeLocalEvent<CP14TradingPortalComponent, StorageAfterOpenEvent>(OnTradePortalOpen);
    }

    private void UpdatePortals(float frameTime)
    {
        var query = EntityQueryEnumerator<CP14TradingPortalComponent, EntityStorageComponent>();
        while (query.MoveNext(out var ent, out var portal, out var storage))
        {
            if (portal.ProcessFinishTime == TimeSpan.Zero || portal.ProcessFinishTime >= _timing.CurTime)
                continue;

            portal.ProcessFinishTime = TimeSpan.Zero;

            SellingThings((ent, portal), storage);
            TopUpBalance((ent, portal), storage);
            BuyThings((ent, portal), storage);
            CashOut((ent, portal), storage);
            ThrowAllItems((ent, portal), storage);
        }
    }

    private void OnTradePortalMapInit(Entity<CP14TradingPortalComponent> ent, ref MapInitEvent args)
    {
        AddRoundstartTradingPositions(ent);
        UpdateStaticPositions(ent);

        ent.Comp.CurrentSpecialBuyPositions.Clear();
        ent.Comp.CurrentSpecialSellPositions.Clear();
        AddRandomBuySpecialPosition(ent, ent.Comp.SpecialBuyPositionCount);
        AddRandomSellSpecialPosition(ent, ent.Comp.SpecialSellPositionCount);
    }

    private void OnTradePortalClose(Entity<CP14TradingPortalComponent> ent, ref StorageAfterCloseEvent args)
    {
        ent.Comp.ProcessFinishTime = _timing.CurTime + ent.Comp.Delay;
    }

    private void OnTradePortalOpen(Entity<CP14TradingPortalComponent> ent, ref StorageAfterOpenEvent args)
    {
        ent.Comp.ProcessFinishTime = TimeSpan.Zero;
    }
}
