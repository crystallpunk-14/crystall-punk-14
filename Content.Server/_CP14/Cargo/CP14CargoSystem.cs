using System.Numerics;
using Content.Server._CP14.Currency;
using Content.Server.Shuttles.Systems;
using Content.Server.Station.Systems;
using Content.Shared._CP14.Cargo;
using Content.Shared._CP14.Cargo.Prototype;
using Content.Shared.Maps;
using Content.Shared.Paper;
using Content.Shared.Storage.EntitySystems;
using Robust.Server.GameObjects;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._CP14.Cargo;

public sealed partial class CP14CargoSystem : CP14SharedCargoSystem
{
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly MapSystem _mapSystem = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly MapLoaderSystem _loader = default!;
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
        InitializePortals();

        _xformQuery = GetEntityQuery<TransformComponent>();

        _buyProto = _proto.EnumeratePrototypes<CP14StoreBuyPositionPrototype>();
        _sellProto = _proto.EnumeratePrototypes<CP14StoreSellPositionPrototype>();

        SubscribeLocalEvent<PrototypesReloadedEventArgs>(OnProtoReload);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        UpdatePortals(frameTime);
    }

    private void OnProtoReload(PrototypesReloadedEventArgs ev)
    {
        _buyProto = _proto.EnumeratePrototypes<CP14StoreBuyPositionPrototype>();
        _sellProto = _proto.EnumeratePrototypes<CP14StoreSellPositionPrototype>();
    }

    private void AddRoundstartTradingPositions(Entity<CP14TradingPortalComponent> portal)
    {
        if (_buyProto is not null)
        {
            foreach (var buy in _buyProto)
            {
                if (buy.RoundstartAvailable)
                    portal.Comp.AvailableBuyPosition.Add(buy);
            }
        }

        if (_sellProto is not null)
        {
            foreach (var sell in _sellProto)
            {
                if (sell.RoundstartAvailable)
                    portal.Comp.AvailableSellPosition.Add(sell);
            }
        }
    }

    private void UpdateStorePositions(Entity<CP14TradingPortalComponent> portal)
    {
        portal.Comp.CurrentBuyPositions.Clear();
        portal.Comp.CurrentSellPositions.Clear();
        portal.Comp.CurrentSpecialBuyPositions.Clear();
        portal.Comp.CurrentSpecialSellPositions.Clear();

        var availableSpecialSellPositions = new List<CP14StoreSellPositionPrototype>();
        var availableSpecialBuyPositions = new List<CP14StoreBuyPositionPrototype>();

        //Add static positions + cash special ones
        foreach (var buyPos in portal.Comp.AvailableBuyPosition)
        {
            if (buyPos.Special)
                availableSpecialBuyPositions.Add(buyPos);
            else
                portal.Comp.CurrentBuyPositions.Add(buyPos, buyPos.Price);
        }

        foreach (var sellPos in portal.Comp.AvailableSellPosition)
        {
            if (sellPos.Special)
                availableSpecialSellPositions.Add(sellPos);
            else
                portal.Comp.CurrentSellPositions.Add(sellPos, sellPos.Price);
        }

        //Random and select special positions
        _random.Shuffle(availableSpecialSellPositions);
        _random.Shuffle(availableSpecialBuyPositions);

        var currentSpecialBuyPositions = portal.Comp.SpecialBuyPositionCount.Next(_random);
        var currentSpecialSellPositions = portal.Comp.SpecialSellPositionCount.Next(_random);

        foreach (var buyPos in availableSpecialBuyPositions)
        {
            if (portal.Comp.CurrentSpecialBuyPositions.Count >= currentSpecialBuyPositions)
                break;
            portal.Comp.CurrentSpecialBuyPositions.Add(buyPos, buyPos.Price);
        }

        foreach (var sellPos in availableSpecialSellPositions)
        {
            if (portal.Comp.CurrentSpecialSellPositions.Count >= currentSpecialSellPositions)
                break;
            portal.Comp.CurrentSpecialSellPositions.Add(sellPos, sellPos.Price);
        }
    }

    /// <summary>
    /// Sell all the items we can, and replenish the internal balance
    /// </summary>
    private void SellingThings(Entity<CP14TradingPortalComponent> portal)
    {
        var ev = new BeforeSellEntities(ref portal.Comp.EntitiesInPortal);
        RaiseLocalEvent(ev);

        foreach (var sellPos in portal.Comp.CurrentSellPositions)
        {
            while (sellPos.Key.Service.TrySell(EntityManager, portal.Comp.EntitiesInPortal))
            {
                portal.Comp.Balance += sellPos.Value;
            }
        }

        foreach (var sellPos in portal.Comp.CurrentSpecialSellPositions)
        {
            while (sellPos.Key.Service.TrySell(EntityManager, portal.Comp.EntitiesInPortal))
            {
                portal.Comp.Balance += sellPos.Value;
            }
        }
    }

    /// <summary>
    /// Take all the money from the portal, and credit it to the internal balance
    /// </summary>
    private void TopUpBalance(Entity<CP14TradingPortalComponent> portal)
    {
        //Get all currency in portal
        var cash = 0;
        foreach (var stored in portal.Comp.EntitiesInPortal)
        {
            var price = _currency.GetTotalCurrency(stored);
            if (price > 0)
            {
                cash += price;
                QueueDel(stored);
            }
        }

        portal.Comp.Balance += cash;
    }

    private void BuyThings(Entity<CP14TradingPortalComponent> portal)
    {
        //Reading all papers in portal
        List<KeyValuePair<CP14StoreBuyPositionPrototype, int>> requests = new();
        foreach (var stored in portal.Comp.EntitiesInPortal)
        {
            if (!TryComp<PaperComponent>(stored, out var paper))
                continue;

            var splittedText = paper.Content.Split("#");
            foreach (var fragment in splittedText)
            {
                foreach (var buyPosition in portal.Comp.CurrentBuyPositions)
                {
                    if (fragment.StartsWith(buyPosition.Key.Code))
                        requests.Add(buyPosition);
                }

                foreach (var buyPosition in portal.Comp.CurrentSpecialBuyPositions)
                {
                    if (fragment.StartsWith(buyPosition.Key.Code))
                        requests.Add(buyPosition);
                }
            }

            QueueDel(stored);
        }

        //Trying to spend inner money to buy requested things
        foreach (var request in requests)
        {
            if (portal.Comp.Balance < request.Value)
                continue;

            portal.Comp.Balance -= request.Value;

            if (!_proto.TryIndex<CP14StoreBuyPositionPrototype>(request.Key, out var indexedBuyed))
                continue;

            foreach (var service in indexedBuyed.Services)
            {
                service.Buy(EntityManager, _proto, portal);
            }
        }
    }

    /// <summary>
    /// Transform all the accumulated balance into physical money, which we will give to the players.
    /// </summary>
    private void CashOut(Entity<CP14TradingPortalComponent> portal)
    {
        var coins = _currency.GenerateMoney(portal.Comp.Balance, GetTradingPoint());
        portal.Comp.EntitiesInPortal.UnionWith(coins);

        portal.Comp.Balance = 0;
    }

    /// <summary>
    /// Return all items to the map
    /// </summary>
    /// <param name="portal"></param>
    private void ReturnAllItems(Entity<CP14TradingPortalComponent> portal)
    {
        foreach (var stored in portal.Comp.EntitiesInPortal)
        {
            _transform.SetCoordinates(stored, Transform(portal).Coordinates.Offset(new Vector2(5, 0)));
        }
        portal.Comp.EntitiesInPortal.Clear();
    }
}
