using Content.Server._CP14.Currency;
using Content.Server.Shuttles.Systems;
using Content.Server.Station.Events;
using Content.Server.Station.Systems;
using Content.Shared._CP14.Currency;
using Content.Shared._CP14.TravelingStoreShip;
using Content.Shared._CP14.TravelingStoreShip.Prototype;
using Content.Shared.Storage.EntitySystems;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._CP14.TravelingStoreShip;

public sealed partial class CP14CargoSystem : CP14SharedCargoSystem
{
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly MapLoaderSystem _loader = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly ShuttleSystem _shuttles = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly UserInterfaceSystem _userInterface = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly CP14CurrencySystem _currency = default!;
    [Dependency] private readonly SharedStorageSystem _storage = default!;

    private EntityQuery<TransformComponent> _xformQuery;

    private IEnumerable<CP14StoreBuyPositionPrototype>? _buyProto;
    private IEnumerable<CP14StoreSellPositionPrototype>? _sellProto;


    public override void Initialize()
    {
        base.Initialize();
        InitializeStore();
        InitializeShuttle();

        _xformQuery = GetEntityQuery<TransformComponent>();

        _buyProto = _proto.EnumeratePrototypes<CP14StoreBuyPositionPrototype>();
        _sellProto = _proto.EnumeratePrototypes<CP14StoreSellPositionPrototype>();

        SubscribeLocalEvent<PrototypesReloadedEventArgs>(OnProtoReload);
        SubscribeLocalEvent<CP14StationTravelingStoreShipTargetComponent, StationPostInitEvent>(OnPostInit);
    }

    private void OnProtoReload(PrototypesReloadedEventArgs ev)
    {
        _buyProto = _proto.EnumeratePrototypes<CP14StoreBuyPositionPrototype>();
        _sellProto = _proto.EnumeratePrototypes<CP14StoreSellPositionPrototype>();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        UpdateShuttle();
    }

    private void OnPostInit(Entity<CP14StationTravelingStoreShipTargetComponent> station, ref StationPostInitEvent args)
    {
        if (!Deleted(station.Comp.Shuttle))
            return;

        var tradepostMap = _mapManager.CreateMap();

        if (!_loader.TryLoad(tradepostMap, station.Comp.ShuttlePath.ToString(), out var shuttleUids))
            return;

        var shuttle =  shuttleUids[0];
        station.Comp.Shuttle = shuttle;
        station.Comp.TradePostMap = _mapManager.GetMapEntityId(tradepostMap);
        var travelingStoreShipComp = EnsureComp<CP14TravelingStoreShipComponent>(station.Comp.Shuttle);
        travelingStoreShipComp.Station = station;

        station.Comp.NextTravelTime = _timing.CurTime + TimeSpan.FromSeconds(10f);
        UpdateStorePositions(station);
    }

    private void UpdateStorePositions(Entity<CP14StationTravelingStoreShipTargetComponent> station)
    {
        station.Comp.CurrentBuyPositions.Clear();
        station.Comp.CurrentSellPositions.Clear();

        if (_buyProto is not null)
        {
            foreach (var buyPos in _buyProto)
            {
                station.Comp.CurrentBuyPositions.Add(buyPos, buyPos.Price.Next(_random));
            }
        }
        if (_sellProto is not null)
        {
            foreach (var sellPos in _sellProto)
            {
                station.Comp.CurrentSellPositions.Add(sellPos, sellPos.Price.Next(_random));
            }
        }
    }

    private void SellingThings(Entity<CP14StationTravelingStoreShipTargetComponent> station)
    {
        var shuttle = station.Comp.Shuttle;

        //Get all entities sent to trading posts
        var toSell = new HashSet<EntityUid>();

        var query = EntityQueryEnumerator<CP14SellingPalettComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out _, out var palletXform))
        {
            if (palletXform.ParentUid != shuttle || !palletXform.Anchored)
                continue;

            var sentEntities = new HashSet<EntityUid>();

            _lookup.GetEntitiesInRange(uid, 1, sentEntities, LookupFlags.Dynamic | LookupFlags.Sundries);

            foreach (var ent in sentEntities)
            {
                if (toSell.Contains(ent) || !_xformQuery.TryGetComponent(ent, out _))
                    continue;

                toSell.Add(ent);
            }
        }

        var cash = 0;
        foreach (var sellPos in station.Comp.CurrentSellPositions)
        {
            if (!_proto.TryIndex(sellPos.Key, out var indexedPos))
                continue;

            while (indexedPos.Service.TrySell(EntityManager, toSell))
            {
                cash += sellPos.Value;
            }
        }

        var moneyBox = GetMoneyBox(station);
        if (moneyBox is not null)
        {
            var coord = Transform(moneyBox.Value).Coordinates;

            if (cash > 0)
            {
                var coins = _currency.GenerateMoney(CP14SharedCurrencySystem.PP.Key, cash, coord, out var remainder);
                cash = remainder;
                foreach (var coin in coins)
                {
                    _storage.Insert(moneyBox.Value, coin, out _);
                }
            }

            if (cash > 0)
            {
                var coins = _currency.GenerateMoney(CP14SharedCurrencySystem.GP.Key, cash, coord, out var remainder);
                cash = remainder;
                foreach (var coin in coins)
                {
                    _storage.Insert(moneyBox.Value, coin, out _);
                }
            }

            if (cash > 0)
            {
                var coins = _currency.GenerateMoney(CP14SharedCurrencySystem.SP.Key, cash, coord, out var remainder);
                cash = remainder;
                foreach (var coin in coins)
                {
                    _storage.Insert(moneyBox.Value, coin, out _);
                }
            }

            if (cash > 0)
            {
                var coins = _currency.GenerateMoney(CP14SharedCurrencySystem.CP.Key, cash, coord);
                foreach (var coin in coins)
                {
                    _storage.Insert(moneyBox.Value, coin, out _);
                }
            }
        }
    }

    private EntityUid? GetMoneyBox(Entity<CP14StationTravelingStoreShipTargetComponent> station)
    {
        var query = EntityQueryEnumerator<CP14CargoMoneyBoxComponent, TransformComponent>();

        while (query.MoveNext(out var uid, out _, out var xform))
        {
            if (xform.GridUid != station.Comp.Shuttle)
                continue;

            return uid;
        }

        return null;
    }
}
