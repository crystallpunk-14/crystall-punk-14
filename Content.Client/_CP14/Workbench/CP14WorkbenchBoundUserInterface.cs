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
    }
}
