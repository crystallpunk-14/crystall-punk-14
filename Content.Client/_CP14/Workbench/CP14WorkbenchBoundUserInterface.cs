/*
 * This file is sublicensed under MIT License
 * https://github.com/space-wizards/space-station-14/blob/master/LICENSE.TXT
 */

using Content.Shared._CP14.Workbench;
using Robust.Client.UserInterface;

namespace Content.Client._CP14.Workbench;

public sealed class CP14WorkbenchBoundUserInterface : BoundUserInterface
{
    private CP14WorkbenchWindow? _window;

    public CP14WorkbenchBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<CP14WorkbenchWindow>();

        _window.OnCraft += entry => SendMessage(new CP14WorkbenchUiCraftMessage(entry.ProtoId));
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        switch (state)
        {
            case CP14WorkbenchUiRecipesState recipesState:
                _window?.UpdateState(recipesState);
                break;
        }
    }
}
