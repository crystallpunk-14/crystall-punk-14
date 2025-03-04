using System.Numerics;
using Content.Shared._CP14.Cargo;
using Content.Shared.Mind.Components;
using Robust.Shared.Map;
using Robust.Shared.Physics.Events;

namespace Content.Server._CP14.Cargo;

public sealed partial class CP14CargoSystem
{
    [Dependency] private readonly MetaDataSystem _meta = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    private EntityUid? _tradingMap = null;
    private void InitializePortals()
    {
        SubscribeLocalEvent<CP14TradingPortalComponent, MapInitEvent>(OnTradePortalMapInit);
        SubscribeLocalEvent<CP14TradingPortalComponent, EntityTerminatingEvent>(OnTradePortalRemove);

        SubscribeLocalEvent<CP14TradingPortalComponent, StartCollideEvent>(OnTradePortalCollide);
    }

    private void UpdatePortals(float frameTime)
    {
        var query = EntityQueryEnumerator<CP14TradingPortalComponent>();
        while (query.MoveNext(out var ent, out var portal))
        {
            if (portal.ProcessFinishTime == TimeSpan.Zero || portal.ProcessFinishTime >= _timing.CurTime)
                continue;

            //FinishProcess
            portal.ProcessFinishTime = TimeSpan.Zero;

            SellingThings((ent, portal));
            TopUpBalance((ent, portal));
            BuyThings((ent, portal));
            CashOut((ent, portal));
            ReturnAllItems((ent, portal));
            UpdateStorePositions((ent, portal));
        }
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

    private void OnTradePortalCollide(Entity<CP14TradingPortalComponent> ent, ref StartCollideEvent args)
    {
        if (HasComp<MindContainerComponent>(args.OtherEntity))
            return;

        _transform.SetCoordinates(args.OtherEntity, GetTradingPoint());
        ent.Comp.EntitiesInPortal.Add(args.OtherEntity);

        RefreshProcessTimer(ent);
    }

    private void RefreshProcessTimer(Entity<CP14TradingPortalComponent> ent)
    {
        ent.Comp.ProcessFinishTime = _timing.CurTime + ent.Comp.Delay;
    }



    public EntityUid GetTradingMap()
    {
        if (_tradingMap is not null)
            return _tradingMap.Value;

        var map = _mapSystem.CreateMap(out var mapId, false);
        _meta.SetEntityName(map, "Trading Map");
        return map;
    }

    public EntityCoordinates GetTradingPoint()
    {
        var map = GetTradingMap();
        return new EntityCoordinates(map, Vector2.Zero);
    }
}
