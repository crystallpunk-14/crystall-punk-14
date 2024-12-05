using Content.Server._CP14.Currency;
using Content.Server.Shuttles.Systems;
using Content.Server.Station.Events;
using Content.Server.Station.Systems;
using Content.Shared._CP14.Currency;
using Content.Shared._CP14.Cargo;
using Content.Shared._CP14.Cargo.Prototype;
using Content.Shared.Maps;
using Content.Shared.Paper;
using Content.Shared.Physics;
using Content.Shared.Storage;
using Content.Shared.Storage.EntitySystems;
using JetBrains.Annotations;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._CP14.Cargo;

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
    [Dependency] private readonly TurfSystem _turf = default!;

    private EntityQuery<TransformComponent> _xformQuery;

    private IEnumerable<CP14StoreBuyPositionPrototype>? _buyProto;
    private IEnumerable<CP14StoreSellPositionPrototype>? _sellProto;


    public override void Initialize()
    {
        base.Initialize();

        InitializeUI();
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
    
    /// <summary>
    /// Allows other systems to additionally add items to the queue that are brought to the settlement on a merchant ship.
    /// </summary>
    [PublicAPI]
    public void AddBuyQueue(Entity<CP14StationTravelingStoreShipTargetComponent> station, List<EntProtoId> products)
    {
        foreach (var product in products)
        {
            station.Comp.BuyedQueue.Enqueue(product);
        }
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
        var travelingStoreShipComp = EnsureComp<CP14TravelingStoreShipComponent>(station.Comp.Shuttle.Value);
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
                station.Comp.CurrentBuyPositions.Add(buyPos, buyPos.Price.Next(_random)/10*10);
            }
        }
        if (_sellProto is not null)
        {
            foreach (var sellPos in _sellProto)
            {
                station.Comp.CurrentSellPositions.Add(sellPos, sellPos.Price.Next(_random)/10*10);
            }
        }
    }

    /// <summary>
    /// Sell all the items we can, and replenish the internal balance
    /// </summary>
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

            _lookup.GetEntitiesInRange(uid, 0.5f, sentEntities, LookupFlags.Dynamic | LookupFlags.Sundries);

            foreach (var ent in sentEntities)
            {
                if (toSell.Contains(ent) || !_xformQuery.TryGetComponent(ent, out _))
                    continue;

                toSell.Add(ent);
            }
        }

        var ev = new BeforeSellEntities(ref toSell);
        RaiseLocalEvent(ev);

        foreach (var sellPos in station.Comp.CurrentSellPositions)
        {
            while (sellPos.Key.Service.TrySell(EntityManager, toSell))
            {
                station.Comp.Balance += sellPos.Value;
            }
        }
    }

    /// <summary>
    /// Take all the money from the tradebox, and credit it to the internal balance
    /// </summary>
    private void TopUpBalance(Entity<CP14StationTravelingStoreShipTargetComponent> station)
    {
        var tradebox = GetTradeBox(station);

        if (tradebox is null)
            return;

        if (!TryComp<StorageComponent>(tradebox, out var tradeStorage))
            return;

        //Get all currency in tradebox
        int cash = 0;
        foreach (var stored in tradeStorage.Container.ContainedEntities)
        {
            var price = _currency.GetTotalCurrency(stored);
            if (price > 0)
            {
                cash += price;
                QueueDel(stored);
            }
        }

        station.Comp.Balance += cash;
    }

    private void BuyToQueue(Entity<CP14StationTravelingStoreShipTargetComponent> station)
    {
        var tradebox = GetTradeBox(station);

        if (tradebox is null)
            return;

        if (!TryComp<StorageComponent>(tradebox, out var tradeStorage))
            return;

        //Reading all papers in tradebox
        List<KeyValuePair<CP14StoreBuyPositionPrototype, int>> requests = new();
        foreach (var stored in tradeStorage.Container.ContainedEntities)
        {
            if (!TryComp<PaperComponent>(stored, out var paper))
                continue;

            var splittedText = paper.Content.Split("#");
            foreach (var fragment in splittedText)
            {
                foreach (var buyPosition in station.Comp.CurrentBuyPositions)
                {
                    if (fragment.StartsWith(buyPosition.Key.Code))
                        requests.Add(buyPosition);
                }
            }

            QueueDel(stored);
        }

        //Trying spend tradebox money to buy requested things
        foreach (var request in requests)
        {
            if (station.Comp.Balance < request.Value)
                continue;

            station.Comp.Balance -= request.Value;

            if (!_proto.TryIndex<CP14StoreBuyPositionPrototype>(request.Key, out var indexedBuyed))
                continue;

            foreach (var service in indexedBuyed.Services)
            {
                service.Buy(EntityManager, station);
            }
        }
    }

    //Dequeue buyed items, and spawn they on shuttle
    private void TrySpawnBuyedThings(Entity<CP14StationTravelingStoreShipTargetComponent> station)
    {
        var shuttle = station.Comp.Shuttle;

        var query = EntityQueryEnumerator<CP14BuyingPalettComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out _, out var palletXform))
        {
            if (station.Comp.BuyedQueue.Count <= 0)
                break;

            if (palletXform.ParentUid != shuttle || !palletXform.Anchored)
                continue;

            var tileRef = palletXform.Coordinates.GetTileRef();
            if (tileRef is null)
                continue;

            if (_turf.IsTileBlocked(tileRef.Value, CollisionGroup.ItemMask))
                continue;

            var buyedThing = station.Comp.BuyedQueue.Dequeue();
            Spawn(buyedThing, palletXform.Coordinates);
        }
    }

    /// <summary>
    /// Transform all the accumulated balance into physical money, which we will give to the players.
    /// </summary>
    private void CashOut(Entity<CP14StationTravelingStoreShipTargetComponent> station)
    {
        var moneyBox = GetTradeBox(station);
        if (moneyBox is not null)
        {
            var coord = Transform(moneyBox.Value).Coordinates;

            if (station.Comp.Balance > 0)
            {
                var coins = _currency.GenerateMoney(CP14SharedCurrencySystem.PP.Key, station.Comp.Balance, coord, out var remainder);
                station.Comp.Balance = remainder;
                foreach (var coin in coins)
                {
                    _storage.Insert(moneyBox.Value, coin, out _);
                }
            }

            if (station.Comp.Balance > 0)
            {
                var coins = _currency.GenerateMoney(CP14SharedCurrencySystem.GP.Key, station.Comp.Balance, coord, out var remainder);
                station.Comp.Balance = remainder;
                foreach (var coin in coins)
                {
                    _storage.Insert(moneyBox.Value, coin, out _);
                }
            }

            if (station.Comp.Balance > 0)
            {
                var coins = _currency.GenerateMoney(CP14SharedCurrencySystem.SP.Key, station.Comp.Balance, coord, out var remainder);
                station.Comp.Balance = remainder;
                foreach (var coin in coins)
                {
                    _storage.Insert(moneyBox.Value, coin, out _);
                }
            }

            if (station.Comp.Balance > 0)
            {
                var coins = _currency.GenerateMoney(CP14SharedCurrencySystem.CP.Key, station.Comp.Balance, coord, out var remainder);
                station.Comp.Balance = remainder;
                foreach (var coin in coins)
                {
                    _storage.Insert(moneyBox.Value, coin, out _);
                }
            }
        }
    }

    private EntityUid? GetTradeBox(Entity<CP14StationTravelingStoreShipTargetComponent> station)
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
