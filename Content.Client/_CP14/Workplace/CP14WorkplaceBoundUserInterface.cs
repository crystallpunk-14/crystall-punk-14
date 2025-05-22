using Content.Shared._CP14.Workplace;
using Robust.Client.UserInterface;

namespace Content.Client._CP14.Workplace;

public sealed class CP14WorkplaceBoundUserInterface : BoundUserInterface
{
    private CP14WorkplaceWindow? _window;

    public CP14WorkplaceBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<CP14WorkplaceWindow>();

        _window.OnCraft += entry => SendMessage(new CP14WorkplaceCraftMessage(entry.Recipe));
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        switch (state)
        {
            case CP14WorkplaceState recipesState:
                _window?.UpdateState(recipesState);
                break;
        }
    }
}
