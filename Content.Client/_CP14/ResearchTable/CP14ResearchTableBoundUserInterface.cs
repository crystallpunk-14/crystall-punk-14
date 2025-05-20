using Content.Shared._CP14.Skill;
using Robust.Client.UserInterface;

namespace Content.Client._CP14.ResearchTable;

public sealed class CP14ResearchTableBoundUserInterface : BoundUserInterface
{
    private CP14ResearchTableWindow? _window;

    public CP14ResearchTableBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<CP14ResearchTableWindow>();

        _window.OnResearch += entry => SendMessage(new CP14ResearchMessage(entry.ProtoId));
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        switch (state)
        {
            case CP14ResearchTableUiState recipesState:
                _window?.UpdateState(recipesState);
                break;
        }
    }
}
