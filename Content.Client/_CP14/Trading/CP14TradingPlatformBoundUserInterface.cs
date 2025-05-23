using Content.Shared._CP14.Trading;
using Content.Shared._CP14.Trading.Systems;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client._CP14.Trading;

public sealed class CP14TradingPlatformBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    private CP14TradingPlatformWindow? _window;

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<CP14TradingPlatformWindow>();

        _window.OnUnlock += pos => SendMessage(new CP14TradingPositionUnlockAttempt(pos));
    }


    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        switch (state)
        {
            case CP14TradingPlatformUiState storeState:
                _window?.UpdateState(storeState);
                break;
        }
    }
}
