using Content.Server.Station.Systems;
using Content.Shared._CP14.Currency;
using Content.Shared._CP14.TravelingStoreShip;
using Content.Shared.UserInterface;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._CP14.TravelingStoreShip;

public sealed class CP14StoreSystem : CP14SharedStoreSystem
{
    [Dependency] private readonly UserInterfaceSystem _userInterface = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly CP14CurrencySystem _cp14Currency = default!;
    [Dependency] private readonly StationSystem _station = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14StoreComponent, BeforeActivatableUIOpenEvent>(OnBeforeUIOpen);
    }

    private void TryInitStore(Entity<CP14StoreComponent> ent)
    {
        if (!TryComp<CP14TravelingStoreShipComponent>(Transform(ent).GridUid, out var travelShip))
            return;

        if (!TryComp<CP14StationTravelingStoreshipTargetComponent>(travelShip.Station, out var station))
            return;

        ent.Comp.Station = new Entity<CP14StationTravelingStoreshipTargetComponent>(travelShip.Station, station);
    }

    private void OnBeforeUIOpen(Entity<CP14StoreComponent> ent, ref BeforeActivatableUIOpenEvent args)
    {
        if (ent.Comp.Station is null)
            TryInitStore(ent);

        UpdateUIProducts(ent);
    }

    private void UpdateUIProducts(Entity<CP14StoreComponent> ent)
    {
        if (ent.Comp.Station is null)
            return;

        var products = new HashSet<CP14StoreUiProductEntry>();

        foreach (var proto in ent.Comp.Station.Value.Comp.CurrentStorePositions)
        {
            products.Add(new CP14StoreUiProductEntry(proto.Key, proto.Value));
        }

        _userInterface.SetUiState(ent.Owner, CP14StoreUiKey.Key, new CP14StoreUiState(products, new(), 150));
    }
}
