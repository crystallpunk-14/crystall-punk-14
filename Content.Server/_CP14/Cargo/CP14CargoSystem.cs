using System.Linq;
using System.Numerics;
using Content.Server._CP14.Currency;
using Content.Server.Storage.Components;
using Content.Server.Storage.EntitySystems;
using Content.Shared._CP14.Cargo;
using Content.Shared._CP14.Cargo.Prototype;
using Content.Shared._CP14.Currency;
using Content.Shared.Paper;
using Content.Shared.Stacks;
using Content.Shared.Throwing;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._CP14.Cargo;

public sealed partial class CP14CargoSystem : CP14SharedCargoSystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly UserInterfaceSystem _userInterface = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly CP14CurrencySystem _currency = default!;
    [Dependency] private readonly EntityStorageSystem _entityStorage = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    private IEnumerable<CP14StoreBuyPositionPrototype>? _buyProto;
    private IEnumerable<CP14StoreSellPositionPrototype>? _sellProto;


    public override void Initialize()
    {
        base.Initialize();

        InitializeUI();
        InitializePortals();

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

    private void UpdateStaticPositions(Entity<CP14TradingPortalComponent> portal)
    {
        portal.Comp.CurrentBuyPositions.Clear();
        portal.Comp.CurrentSellPositions.Clear();

        //Add static positions + cash special ones
        foreach (var buyPos in portal.Comp.AvailableBuyPosition)
        {
            if (buyPos.Special)
                continue;

            if (buyPos.Factions.Count > 0 && !buyPos.Factions.Contains(portal.Comp.Faction))
                continue;

            portal.Comp.CurrentBuyPositions.Add(buyPos, buyPos.Price);
        }

        foreach (var sellPos in portal.Comp.AvailableSellPosition)
        {
            if (sellPos.Special)
                continue;

            if (sellPos.Factions.Count > 0 && !sellPos.Factions.Contains(portal.Comp.Faction))
                continue;

            portal.Comp.CurrentSellPositions.Add(sellPos, sellPos.Price);
        }
    }

    public void AddRandomBuySpecialPosition(Entity<CP14TradingPortalComponent> portal, int count)
    {
        if (_buyProto is null)
            return;

        var availableSpecialBuyPositions = new List<CP14StoreBuyPositionPrototype>();
        foreach (var buyPos in _buyProto)
        {
            if (!buyPos.Special)
                continue;

            if (portal.Comp.CurrentSpecialBuyPositions.ContainsKey(buyPos))
                continue;

            if (buyPos.Factions.Count > 0 && !buyPos.Factions.Contains(portal.Comp.Faction))
                continue;

            availableSpecialBuyPositions.Add(buyPos);
        }

        _random.Shuffle(availableSpecialBuyPositions);

        var added = 0;
        foreach (var buyPos in availableSpecialBuyPositions)
        {
            if (added >= count)
                break;
            portal.Comp.CurrentSpecialBuyPositions.Add(buyPos, buyPos.Price);
            added++;
        }
    }

    public void AddRandomSellSpecialPosition(Entity<CP14TradingPortalComponent> portal, int count)
    {
        if (_sellProto is null)
            return;

        var availableSpecialSellPositions = new List<CP14StoreSellPositionPrototype>();
        foreach (var sellPos in _sellProto)
        {
            if (!sellPos.Special)
                continue;

            if (portal.Comp.CurrentSpecialSellPositions.ContainsKey(sellPos))
                continue;

            if (sellPos.Factions.Count > 0 && !sellPos.Factions.Contains(portal.Comp.Faction))
                continue;

            availableSpecialSellPositions.Add(sellPos);
        }

        _random.Shuffle(availableSpecialSellPositions);

        var added = 0;
        foreach (var sellPos in availableSpecialSellPositions)
        {
            if (added >= count)
                break;
            portal.Comp.CurrentSpecialSellPositions.Add(sellPos, sellPos.Price);
            added++;
        }
    }

    /// <summary>
    /// Sell all the items we can, and replenish the internal balance
    /// </summary>
    private void SellingThings(Entity<CP14TradingPortalComponent> portal, EntityStorageComponent storage)
    {
        var containedEntities = storage.Contents.ContainedEntities.ToHashSet();
        //var ev = new BeforeSellEntities(ref portal.Comp.EntitiesInPortal);
        //RaiseLocalEvent(ev);

        foreach (var sellPos in portal.Comp.CurrentSellPositions)
        {
            //WHILE = sell all we can
            while (sellPos.Key.Service.TrySell(EntityManager, containedEntities))
            {
                portal.Comp.Balance += sellPos.Value;
            }
        }

        List<CP14StoreSellPositionPrototype> toRemove = new();
        foreach (var sellPos in portal.Comp.CurrentSpecialSellPositions)
        {
            //IF = only 1 try
            if (sellPos.Key.Service.TrySell(EntityManager, containedEntities))
            {
                portal.Comp.Balance += sellPos.Value;
                toRemove.Add(sellPos.Key);
            }
        }

        //Remove this special position from the list and add new random one
        foreach (var position in toRemove)
        {
            portal.Comp.CurrentSpecialSellPositions.Remove(position);
            AddRandomSellSpecialPosition(portal, 1);
        }
    }

    /// <summary>
    /// Take all the money from the portal, and credit it to the internal balance
    /// </summary>
    private void TopUpBalance(Entity<CP14TradingPortalComponent> portal, EntityStorageComponent storage)
    {
        //Get all currency in portal
        var cash = 0;
        foreach (var stored in storage.Contents.ContainedEntities)
        {
            if (TryComp<CP14CurrencyComponent>(stored, out var currency))
            {
                //fix currency calculation
                var c = currency.Currency;

                if (TryComp<StackComponent>(stored, out var stack))
                    c *= stack.Count;

                cash += c;
                QueueDel(stored);
            }
        }

        portal.Comp.Balance += cash;
    }

    private void BuyThings(Entity<CP14TradingPortalComponent> portal, EntityStorageComponent storage)
    {
        //Reading all papers in portal
        List<KeyValuePair<CP14StoreBuyPositionPrototype, int>> requests = new();
        foreach (var stored in storage.Contents.ContainedEntities)
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

            //Remove this position from the list and add new random one
            if (request.Key.Special)
            {
                portal.Comp.CurrentSpecialBuyPositions.Remove(request.Key);
                AddRandomBuySpecialPosition(portal, 1);
            }

            indexedBuyed.Service.Buy(EntityManager, _proto, portal);
        }
    }

    /// <summary>
    /// Transform all the accumulated balance into physical money, which we will give to the players.
    /// </summary>
    private void CashOut(Entity<CP14TradingPortalComponent> portal, EntityStorageComponent storage)
    {
        var coins = _currency.GenerateMoney(portal.Comp.Balance, Transform(portal).Coordinates);
        foreach (var coin in coins)
        {
            _entityStorage.Insert(coin, portal, storage);
        }
        portal.Comp.Balance = 0;
    }

    /// <summary>
    /// Return all items to the map
    /// </summary>
    private void ThrowAllItems(Entity<CP14TradingPortalComponent> portal, EntityStorageComponent storage)
    {
        var containedEntities = storage.Contents.ContainedEntities.ToList();

        _entityStorage.OpenStorage(portal, storage);

        var xform = Transform(portal);
        var rotation = xform.LocalRotation;
        foreach (var stored in containedEntities)
        {
            _transform.AttachToGridOrMap(stored);
            var targetThrowPosition = xform.Coordinates.Offset(rotation.ToWorldVec() * 1);
            _throwing.TryThrow(stored, targetThrowPosition.Offset(new Vector2(_random.NextFloat(-0.5f, 0.5f), _random.NextFloat(-0.5f, 0.5f))));
        }
    }
}
