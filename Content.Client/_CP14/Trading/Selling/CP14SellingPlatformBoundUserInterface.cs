using Content.Shared._CP14.Trading;
using Content.Shared._CP14.Trading.Systems;
using Robust.Client.UserInterface;

namespace Content.Client._CP14.Trading.Selling;

public sealed class CP14SellingPlatformBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    private CP14SellingPlatformWindow? _window;

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<CP14SellingPlatformWindow>();

        _window.OnSell += () => SendMessage(new CP14TradingSellAttempt());
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        switch (state)
        {
            case CP14SellingPlatformUiState storeState:
                _window?.UpdateState(storeState);
                break;
        }
    }
}
