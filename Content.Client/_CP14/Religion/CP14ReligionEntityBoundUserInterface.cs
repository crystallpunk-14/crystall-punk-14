using Content.Client._CP14.DemiplaneTraveling;
using Content.Shared._CP14.DemiplaneTraveling;
using Content.Shared._CP14.Religion.Systems;
using Robust.Client.UserInterface;

namespace Content.Client._CP14.Religion;

public sealed class CP14ReligionEntityBoundUserInterface : BoundUserInterface
{
    private CP14ReligionEntityWindow? _window;

    public CP14ReligionEntityBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<CP14ReligionEntityWindow>();

        _window.OnTeleportAttempt += netId => SendMessage(new CP14ReligionEntityTeleportAttempt(netId));
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (_window == null || state is not CP14ReligionEntityUiState mapState)
            return;

        _window?.UpdateState(mapState);
    }
}
