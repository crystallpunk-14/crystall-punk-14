using Content.Server._CP14.Currency;
using Content.Server.Cargo.Systems;
using Content.Server.Station.Events;
using Content.Server.Storage.EntitySystems;
using Content.Shared._CP14.Cargo;
using Content.Shared._CP14.Cargo.Prototype;
using Content.Shared.Station.Components;
using Content.Shared.Throwing;
using Robust.Server.GameObjects;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._CP14.Cargo;

public sealed partial class CP14CargoSystem : CP14SharedCargoSystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly UserInterfaceSystem _userInterface = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly PricingSystem _price = default!;
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly MapLoaderSystem _loader = default!;

    private EntityQuery<TransformComponent> _xformQuery;

    private IEnumerable<CP14StoreBuyPositionPrototype>? _buyProto;

    public override void Initialize()
    {
        base.Initialize();

        InitializeUI();
        InitializeShuttle();

        _xformQuery = GetEntityQuery<TransformComponent>();
        _buyProto = _proto.EnumeratePrototypes<CP14StoreBuyPositionPrototype>();

        SubscribeLocalEvent<PrototypesReloadedEventArgs>(OnProtoReload);
        SubscribeLocalEvent<CP14StationTravelingStoreShipComponent, StationPostInitEvent>(OnPostInit);
    }

    private void OnProtoReload(PrototypesReloadedEventArgs ev)
    {
        _buyProto = _proto.EnumeratePrototypes<CP14StoreBuyPositionPrototype>();
    }

    private void OnPostInit(Entity<CP14StationTravelingStoreShipComponent> station, ref StationPostInitEvent args)
    {
        if (!Deleted(station.Comp.Shuttle))
            return;

        var tradepostMap = _mapSystem.CreateMap(out var mapId);

        if (!_loader.TryLoadGrid(mapId ,station.Comp.ShuttlePath, out var shuttle))
            return;

        station.Comp.Shuttle = shuttle;
        station.Comp.TradePostMap = tradepostMap;
        var travelingStoreShipComp = EnsureComp<CP14TravelingStoreShipComponent>(station.Comp.Shuttle.Value);
        travelingStoreShipComp.Station = station;

        var member = EnsureComp<StationMemberComponent>(shuttle.Value);
        member.Station = station;

        AddRoundstartTradingPositions(station);
        UpdateStaticPositions(station);

        SendShuttleToStation(station.Comp.Shuttle.Value);
    }

    private void AddRoundstartTradingPositions(Entity<CP14StationTravelingStoreShipComponent> station)
    {
        if (_buyProto is not null)
        {
            foreach (var buy in _buyProto)
            {
                if (buy.RoundstartAvailable)
                    station.Comp.AvailableBuyPosition.Add(buy);
            }
        }
    }

    private void UpdateStaticPositions(Entity<CP14StationTravelingStoreShipComponent> station)
    {
        station.Comp.CurrentBuyPositions.Clear();

        //Add static positions + cash special ones
        foreach (var buyPos in station.Comp.AvailableBuyPosition)
        {
            if (buyPos.Special)
                continue;

            station.Comp.CurrentBuyPositions.Add(buyPos, buyPos.Price);
        }
    }

    public void AddRandomBuySpecialPosition(Entity<CP14StationTravelingStoreShipComponent> station, int count)
    {
        if (_buyProto is null)
            return;

        var availableSpecialBuyPositions = new List<CP14StoreBuyPositionPrototype>();
        foreach (var buyPos in _buyProto)
        {
            if (!buyPos.Special)
                continue;

            if (station.Comp.CurrentSpecialBuyPositions.ContainsKey(buyPos))
                continue;

            availableSpecialBuyPositions.Add(buyPos);
        }

        _random.Shuffle(availableSpecialBuyPositions);

        var added = 0;
        foreach (var buyPos in availableSpecialBuyPositions)
        {
            if (added >= count)
                break;
            station.Comp.CurrentSpecialBuyPositions.Add(buyPos, buyPos.Price);
            added++;
        }
    }

    /// <summary>
    /// Sell all the items we can, and replenish the internal balance
    /// </summary>
    private void SellingThings(Entity<CP14StationTravelingStoreShipComponent> station)
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

        foreach (var ent in toSell)
        {
            var price = _price.GetPrice(ent);
            if (price <= 0)
                continue;

            station.Comp.Balance += (int)price;
        }
    }

    /// <summary>
    /// Transform all the accumulated balance into physical money, which we will give to the players.
    /// </summary>
    //private void CashOut(Entity<CP14TradingPortalComponent> portal, EntityStorageComponent storage)
    //{
    //    var coins = _currency.GenerateMoney(portal.Comp.Balance, Transform(portal).Coordinates);
    //    foreach (var coin in coins)
    //    {
    //        _entityStorage.Insert(coin, portal, storage);
    //    }
    //    portal.Comp.Balance = 0;
    //}

    /// <summary>
    /// Return all items to the map
    /// </summary>
    //private void ThrowAllItems(Entity<CP14TradingPortalComponent> portal, EntityStorageComponent storage)
    //{
    //    var containedEntities = storage.Contents.ContainedEntities.ToList();
//
    //    _entityStorage.OpenStorage(portal, storage);
//
    //    var xform = Transform(portal);
    //    var rotation = xform.LocalRotation;
    //    foreach (var stored in containedEntities)
    //    {
    //        _transform.AttachToGridOrMap(stored);
    //        var targetThrowPosition = xform.Coordinates.Offset(rotation.ToWorldVec() * 1);
    //        _throwing.TryThrow(stored, targetThrowPosition.Offset(new Vector2(_random.NextFloat(-0.5f, 0.5f), _random.NextFloat(-0.5f, 0.5f))));
    //    }
    //}
}
