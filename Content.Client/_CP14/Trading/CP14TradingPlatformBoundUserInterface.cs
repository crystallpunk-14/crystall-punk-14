using Content.Shared._CP14.Trading;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client._CP14.Trading;

public sealed class CP14TradingPlatformBoundUserInterface : BoundUserInterface
{
    private CP14TradingPlatformWindow? _window;

    public CP14TradingPlatformBoundUserInterface(EntityUid owner, [NotNull] Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<CP14TradingPlatformWindow>();
    }


    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        switch (state)
        {
            case CP14TradingPlatformUiState storeState:
                _window?.UpdateUI(storeState);
                break;
        }
    }
}
