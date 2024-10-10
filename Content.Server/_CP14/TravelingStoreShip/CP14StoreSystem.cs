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

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14StoreComponent, BeforeActivatableUIOpenEvent>(OnBeforeUIOpen);
    }


    private void OnBeforeUIOpen(Entity<CP14StoreComponent> ent, ref BeforeActivatableUIOpenEvent args)
    {
        UpdateUIProducts(ent);
    }

    private void UpdateUIProducts(Entity<CP14StoreComponent> ent)
    {
        var products = new HashSet<CP14StoreUiProductEntry>();

        foreach (var proto in _proto.EnumeratePrototypes<CP14StoreBuyPositionPrototype>())
        {
            products.Add(new CP14StoreUiProductEntry(proto.ID, proto.Price.Next(_random)));
        }
        _userInterface.SetUiState(ent.Owner, CP14StoreUiKey.Key, new CP14StoreUiState(products));
    }
}
