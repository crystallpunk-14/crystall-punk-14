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

    public override void Initialize()
    {
        base.Initialize();

        _xformQuery = GetEntityQuery<TransformComponent>();

        InitializeStore();
        InitializeShuttle();

        SubscribeLocalEvent<CP14StationTravelingStoreShipTargetComponent, StationPostInitEvent>(OnPostInit);
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

        SendShuttleToStation(station); //Start fast travel
        UpdateStorePositions(station);
    }

    private void UpdateStorePositions(Entity<CP14StationTravelingStoreShipTargetComponent> station)
    {
        station.Comp.CurrentBuyPositions.Clear();
        station.Comp.CurrentSellPositions.Clear();

        //Static add
        foreach (var position in station.Comp.StaticBuyPositions)
        {
            if (!_proto.TryIndex(position, out var indexedP))
                continue;
            station.Comp.CurrentBuyPositions.Add(position, (indexedP.Price.Next(_random), false));
        }
        foreach (var position in station.Comp.StaticSellPositions)
        {
            if (!_proto.TryIndex(position, out var indexedP))
                continue;
            station.Comp.CurrentSellPositions.Add(position, (indexedP.Price.Next(_random), false));
        }

        //Dynamic add
        var specialsBuy = MathF.Min(station.Comp.SpecialBuyPositionCount.Next(_random), station.Comp.DynamicBuyPositions.Count);
        var tmpListBuy = new List<ProtoId<CP14StoreBuyPositionPrototype>>(station.Comp.DynamicBuyPositions);
        for (var i = 0; i < specialsBuy; i++)
        {
            var rand = _random.Pick(tmpListBuy);
            tmpListBuy.Remove(rand);
            if (!_proto.TryIndex(rand, out var indexedP))
                continue;
            station.Comp.CurrentBuyPositions.Add(rand, (indexedP.Price.Next(_random), true));
        }

        var specialsSell = MathF.Min(station.Comp.SpecialSellPositionCount.Next(_random), station.Comp.DynamicSellPositions.Count);
        var tmpListSell = new List<ProtoId<CP14StoreSellPositionPrototype>>(station.Comp.DynamicSellPositions);
        for (var i = 0; i < specialsSell; i++)
        {
            var rand = _random.Pick(tmpListSell);
            tmpListSell.Remove(rand);
            if (!_proto.TryIndex(rand, out var indexedP))
                continue;
            station.Comp.CurrentSellPositions.Add(rand, (indexedP.Price.Next(_random), true));
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
                cash += sellPos.Value.Item1;
            }
        }

        var moneyBox = GetMoneyBox(station);
        if (moneyBox is not null)
        {
            var coord = Transform(moneyBox.Value).Coordinates;

            if (cash > 0)
            {
                var coins = _currency.GenerateMoney(CP14SharedCurrencySystem.PP.Key, cash, CP14SharedCurrencySystem.PP.Value, coord, out var remainder);
                cash = remainder;
                foreach (var coin in coins)
                {
                    _storage.Insert(moneyBox.Value, coin, out _);
                }
            }

            if (cash > 0)
            {
                var coins = _currency.GenerateMoney(CP14SharedCurrencySystem.GP.Key, cash, CP14SharedCurrencySystem.GP.Value, coord, out var remainder);
                cash = remainder;
                foreach (var coin in coins)
                {
                    _storage.Insert(moneyBox.Value, coin, out _);
                }
            }

            if (cash > 0)
            {
                var coins = _currency.GenerateMoney(CP14SharedCurrencySystem.SP.Key, cash, CP14SharedCurrencySystem.SP.Value, coord, out var remainder);
                cash = remainder;
                foreach (var coin in coins)
                {
                    _storage.Insert(moneyBox.Value, coin, out _);
                }
            }

            if (cash > 0)
            {
                var coins = _currency.GenerateMoney(CP14SharedCurrencySystem.CP.Key, cash, CP14SharedCurrencySystem.CP.Value, coord, out _);
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
