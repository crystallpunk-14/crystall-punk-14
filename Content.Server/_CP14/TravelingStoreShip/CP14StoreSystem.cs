using Content.Shared._CP14.TravelingStoreShip;
using Content.Shared.UserInterface;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Server._CP14.TravelingStoreShip;

public sealed class CP14StoreSystem : CP14SharedStoreSystem
{
    [Dependency] private readonly UserInterfaceSystem _userInterface = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    HashSet<CP14StoreBuyPositionPrototype> _products = new ();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CP14StoreComponent, BeforeActivatableUIOpenEvent>(OnBeforeUIOpen);

        foreach (var proto in _proto.EnumeratePrototypes<CP14StoreBuyPositionPrototype>())
        {
            _products.Add(proto);
        }
    }

    private void OnBeforeUIOpen(Entity<CP14StoreComponent> ent, ref BeforeActivatableUIOpenEvent args)
    {
        UpdateUIProducts(ent);
    }

    private void UpdateUIProducts(Entity<CP14StoreComponent> ent)
    {
        var products = new HashSet<CP14StoreUiProductEntry>();
        foreach (var product in _products)
        {
            products.Add(new CP14StoreUiProductEntry(product));
        }
        _userInterface.SetUiState(ent.Owner, CP14StoreUiKey.Key, new CP14StoreUiState(products));
    }
}
