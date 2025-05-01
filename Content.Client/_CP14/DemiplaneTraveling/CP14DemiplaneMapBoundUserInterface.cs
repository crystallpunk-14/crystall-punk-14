using Content.Shared._CP14.DemiplaneTraveling;
using Robust.Client.UserInterface;

namespace Content.Client._CP14.DemiplaneTraveling;

public sealed class CP14DemiplaneMapBoundUserInterface : BoundUserInterface
{
    private CP14DemiplaneMapWindow? _window;

    public CP14DemiplaneMapBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<CP14DemiplaneMapWindow>();

        //_window.OnCraft += entry => SendMessage(new CP14WorkbenchUiCraftMessage(entry.ProtoId));
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (_window == null || state is not CP14DemiplaneMapUiState mapState)
            return;

        _window?.UpdateState(mapState);
    }
}
