using Content.Server._CP14.Currency;
using Content.Server.Cargo.Systems;
using Content.Server.Storage.Components;
using Content.Shared._CP14.Trading;
using Content.Shared._CP14.Trading.Components;
using Content.Shared._CP14.Trading.Prototypes;
using Content.Shared._CP14.Trading.Systems;
using Content.Shared.Mobs.Components;
using Content.Shared.Placeable;
using Content.Shared.Popups;
using Content.Shared.Storage;
using Content.Shared.Tag;
using Content.Shared.UserInterface;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.Trading;

public sealed partial class CP14TradingPlatformSystem : CP14SharedTradingPlatformSystem
{
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly PricingSystem _price = default!;
    [Dependency] private readonly CP14CurrencySystem _cp14Currency = default!;
    [Dependency] private readonly CP14StationEconomySystem _economy = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly UserInterfaceSystem _userInterface = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14TradingPlatformComponent, CP14TradingPositionBuyAttempt>(OnBuyAttempt);

        SubscribeLocalEvent<CP14SellingPlatformComponent, BeforeActivatableUIOpenEvent>(OnBeforeSellingUIOpen);
        SubscribeLocalEvent<CP14SellingPlatformComponent, ItemPlacedEvent>(OnItemPlaced);
        SubscribeLocalEvent<CP14SellingPlatformComponent, ItemRemovedEvent>(OnItemRemoved);

        SubscribeLocalEvent<CP14SellingPlatformComponent, CP14TradingSellAttempt>(OnSellAttempt);
        SubscribeLocalEvent<CP14SellingPlatformComponent, CP14TradingRequestSellAttempt>(OnSellRequestAttempt);
    }

    private void OnSellAttempt(Entity<CP14SellingPlatformComponent> ent, ref CP14TradingSellAttempt args)
    {
        if (!TryComp<ItemPlacerComponent>(ent, out var itemPlacer))
            return;

        double balance = 0;
        foreach (var placed in itemPlacer.PlacedEntities)
        {
            if (!CanSell(placed))
                continue;

            var price = _price.GetPrice(placed);

            if (price <= 0)
                continue;

            balance += _price.GetPrice(placed);
            QueueDel(placed);
        }

        if (balance <= 0)
            return;

        _audio.PlayPvs(ent.Comp.SellSound, Transform(ent).Coordinates);
        _cp14Currency.GenerateMoney(balance, Transform(ent).Coordinates);
        SpawnAtPosition(ent.Comp.SellVisual, Transform(ent).Coordinates);

        UpdateSellingUIState(ent);
    }

    private void OnSellRequestAttempt(Entity<CP14SellingPlatformComponent> ent, ref CP14TradingRequestSellAttempt args)
    {
        if (!TryComp<ItemPlacerComponent>(ent, out var itemPlacer))
            return;

        if (!CanFulfillRequest(ent, args.Request))
            return;

        if (!Proto.TryIndex(args.Request, out var indexedRequest))
            return;

        foreach (var req in indexedRequest.Requirements)
        {
            req.PostCraft(EntityManager, Proto, itemPlacer.PlacedEntities, null);
        }

        _audio.PlayPvs(ent.Comp.SellSound, Transform(ent).Coordinates);
        _cp14Currency.GenerateMoney(_economy.GetPrice(indexedRequest) ?? 0, Transform(ent).Coordinates);
        AddReputation(args.Actor, args.Faction, indexedRequest.ReputationReward);
        SpawnAtPosition(ent.Comp.SellVisual, Transform(ent).Coordinates);



        UpdateSellingUIState(ent);
    }

    private void OnItemRemoved(Entity<CP14SellingPlatformComponent> ent, ref ItemRemovedEvent args)
    {
        UpdateSellingUIState(ent);
    }

    private void OnItemPlaced(Entity<CP14SellingPlatformComponent> ent, ref ItemPlacedEvent args)
    {
        UpdateSellingUIState(ent);
    }

    private void OnBuyAttempt(Entity<CP14TradingPlatformComponent> ent, ref CP14TradingPositionBuyAttempt args)
    {
        TryBuyPosition(args.Actor, ent, args.Position);
        UpdateTradingUIState(ent, args.Actor);
    }

    private void OnBeforeSellingUIOpen(Entity<CP14SellingPlatformComponent> ent, ref BeforeActivatableUIOpenEvent args)
    {
        UpdateSellingUIState(ent);
    }

    private void UpdateSellingUIState(Entity<CP14SellingPlatformComponent> ent)
    {
        if (!TryComp<ItemPlacerComponent>(ent, out var itemPlacer))
            return;

        //Calculate
        double balance = 0;
        foreach (var placed in itemPlacer.PlacedEntities)
        {
            if (!CanSell(placed))
                continue;

            balance += _price.GetPrice(placed);
        }

        _userInterface.SetUiState(ent.Owner, CP14TradingUiKey.Sell, new CP14SellingPlatformUiState(GetNetEntity(ent), (int)balance));
    }

    public bool CanSell(EntityUid uid)
    {
        if (_tag.HasTag(uid, "CP14Coin")) //Boo hardcoding
            return false;
        if (HasComp<MobStateComponent>(uid))
            return false;
        if (HasComp<EntityStorageComponent>(uid))
            return false;
        if (HasComp<StorageComponent>(uid))
            return false;

        var proto = MetaData(uid).EntityPrototype;
        if (proto != null && !proto.ID.StartsWith("CP14")) //Shitfix, we dont wanna sell anything vanilla (like mob organs)
            return false;

        return true;
    }

    public bool TryBuyPosition(Entity<CP14TradingReputationComponent?> user, Entity<CP14TradingPlatformComponent> platform, ProtoId<CP14TradingPositionPrototype> position)
    {
        if (Timing.CurTime < platform.Comp.NextBuyTime)
            return false;

        if (!CanBuyPosition(user, position))
            return false;

        if (!Proto.TryIndex(position, out var indexedPosition))
            return false;

        if (!Resolve(user.Owner, ref user.Comp, false))
            return false;

        if (!TryComp<ItemPlacerComponent>(platform, out var itemPlacer))
            return false;

        //Top up balance
        double balance = 0;
        foreach (var placedEntity in itemPlacer.PlacedEntities)
        {
            if (!_tag.HasTag(placedEntity, platform.Comp.CoinTag))
                continue;
            balance += _price.GetPrice(placedEntity);
        }

        var price = _economy.GetPrice(position) ?? 10000;
        if (balance < price)
        {
            // Not enough balance to buy the position
            _popup.PopupEntity(Loc.GetString("cp14-trading-failure-popup-money"), platform);
            return false;
        }

        foreach (var placedEntity in itemPlacer.PlacedEntities)
        {
            if (!_tag.HasTag(placedEntity, platform.Comp.CoinTag))
                continue;
            QueueDel(placedEntity);
        }

        balance -= price;

        platform.Comp.NextBuyTime = Timing.CurTime + TimeSpan.FromSeconds(1f);
        Dirty(platform);

        if (indexedPosition.Service is not null)
            indexedPosition.Service.Buy(EntityManager, Proto, platform);

        AddReputation(user, indexedPosition.Faction, (float)price / 100);

        _audio.PlayPvs(platform.Comp.BuySound, Transform(platform).Coordinates);

        //return the change
        _cp14Currency.GenerateMoney(balance, Transform(platform).Coordinates);
        SpawnAtPosition(platform.Comp.BuyVisual, Transform(platform).Coordinates);
        return true;
    }
}
