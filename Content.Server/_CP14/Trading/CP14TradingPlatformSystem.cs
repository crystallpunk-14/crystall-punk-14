using Content.Server._CP14.Currency;
using Content.Server._CP14.MagicEnergy;
using Content.Server.Cargo.Systems;
using Content.Shared._CP14.MagicEnergy;
using Content.Shared._CP14.Trading.Components;
using Content.Shared._CP14.Trading.Prototypes;
using Content.Shared._CP14.Trading.Systems;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Placeable;
using Content.Shared.Popups;
using Content.Shared.Tag;
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
    [Dependency] private readonly CP14MagicEnergySystem _magicEnergy = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14TradingPlatformComponent, CP14TradingPositionBuyAttempt>(OnBuyAttempt);
        SubscribeLocalEvent<CP14SellingPlatformComponent, CP14MagicEnergyLevelChangeEvent>(OnMagicChange);
    }

    private void OnMagicChange(Entity<CP14SellingPlatformComponent> ent, ref CP14MagicEnergyLevelChangeEvent args)
    {
        if (args.NewValue != args.MaxValue)
            return;

        _magicEnergy.ClearEnergy(ent.Owner);

        if (!TryComp<ItemPlacerComponent>(ent, out var itemPlacer))
            return;

        double price = 0;
        foreach (var placed in itemPlacer.PlacedEntities)
        {
            if (HasComp<MobStateComponent>(placed))
                continue;

            var placedPrice = _price.GetPrice(placed);

            if (placedPrice <= 0)
                continue;

            price += placedPrice;
            QueueDel(placed);
        }

        _audio.PlayPvs(ent.Comp.SellSound, Transform(ent).Coordinates);
        _cp14Currency.GenerateMoney(price, Transform(ent).Coordinates);
        SpawnAtPosition(ent.Comp.SellVisual, Transform(ent).Coordinates);
    }

    private void OnBuyAttempt(Entity<CP14TradingPlatformComponent> ent, ref CP14TradingPositionBuyAttempt args)
    {
        TryBuyPosition(args.Actor, ent, args.Position);
        UpdateUIState(ent, args.Actor);
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
