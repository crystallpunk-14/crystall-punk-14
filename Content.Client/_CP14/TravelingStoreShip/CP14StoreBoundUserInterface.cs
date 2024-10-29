using Content.Shared._CP14.Cargo;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client._CP14.TravelingStoreShip;

public sealed class CP14StoreBoundUserInterface : BoundUserInterface
{
    private CP14StoreWindow? _window;

    public CP14StoreBoundUserInterface(EntityUid owner, [NotNull] Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<CP14StoreWindow>();
    }


    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        switch (state)
        {
            case CP14StoreUiState storeState:
                _window?.UpdateUI(storeState);
                break;
        }
    }
}
